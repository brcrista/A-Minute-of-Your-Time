import dateutil
import functools
import json
import os

import faker
import numpy as np
import pandas as pd

from . import text_helpers

def _ensure_camel(s):
    """
    Convert a string to camel case.
    Some of the JSON properties in the response from the Azure DevOps API are not camel-cased.
    """
    allowed_names = ['_links']
    return s if text_helpers.iscamel(s) or s in allowed_names else text_helpers.camel(s)

def _read_json_file(filepath):
    try:
        with open(filepath, 'r', encoding='utf-8') as pull_requests_json_file:
            return json.load(pull_requests_json_file, object_hook=lambda d: text_helpers.remap_keys(_ensure_camel, d))
    except FileNotFoundError:
        return None

_fake_authors = {}
_faker = faker.Faker()

def _files_changed(change_jobject):
    change_list = change_jobject['changes']
    return [x for x in change_list if x['item']['gitObjectType'] == 3]

# Create a data frame of pull requests
def _get_data_from_pull_request(data_directory, pull_request):
    """
    Extract the information we want to process from a pull request API object.
    """
    # Translate the author to a fake name
    real_author = pull_request['createdBy']['displayName']
    if real_author in _fake_authors:
        fake_author = _fake_authors[real_author]
    else:
        fake_author = _faker.name()
        _fake_authors[real_author] = fake_author

    # Load iterations
    pull_request_id = pull_request['pullRequestId']

    # See if we have a data file for iterations; otherwise use NaN
    # For the filename format, see the FetchPullRequestData tool
    iterations_filename = os.path.join(data_directory, f'{pull_request_id}-iterations.json')
    iterations = _read_json_file(iterations_filename)
    num_iterations = len(iterations) if iterations else np.NaN

    # See if we have a data file for changes; otherwise use NaN
    # For the filename format, see the FetchPullRequestData tool
    changes_filename = os.path.join(data_directory, f'{pull_request_id}-changes.json')
    changes = _read_json_file(changes_filename)
    num_files_changed = len(_files_changed(changes)) if changes else np.NaN

    return [
        pull_request_id, # id
        fake_author, # author
        dateutil.parser.parse(pull_request['creationDate']), # created_time
        dateutil.parser.parse(pull_request['closedDate']), # merged_time
        len(pull_request['reviewers']), # num_reviewers
        num_iterations, # num_iterations
        num_files_changed # num_files_changed
    ]

def load_data(filepath):
    """
    Parse the JSON file and convert to a Pandas dataframe with the information we need.
    """
    pull_requests_json = _read_json_file(filepath)

    get_data = functools.partial(
        _get_data_from_pull_request,
        os.path.dirname(filepath))

    pull_requests = pd.DataFrame(
        [get_data(pr) for pr in pull_requests_json],
        columns=['id', 'author', 'created_time', 'merged_time', 'num_reviewers', 'num_iterations', 'num_files_changed'])

    # Add a column for wall-clock time to complete
    pull_requests['ttl'] = pull_requests['merged_time'] - pull_requests['created_time']
    return pull_requests