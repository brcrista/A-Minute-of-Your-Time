import dateutil
import json

import faker
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
    with open(filepath, 'r', encoding='utf-8') as pull_requests_json_file:
        return json.load(pull_requests_json_file, object_hook=lambda d: text_helpers.remap_keys(_ensure_camel, d))

_fake_authors = {}
_faker = faker.Faker()

# Create a data frame of pull requests
def _get_data_from_pull_request(pull_request):
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

    return [
        pull_request['pullRequestId'], # id
        fake_author, # author
        dateutil.parser.parse(pull_request['creationDate']), # created_time
        dateutil.parser.parse(pull_request['closedDate']), # merged_time
        len(pull_request['reviewers']) # num_reviewers
    ]

def load_data(filepath):
    """
    Parse the JSON file and convert to a Pandas dataframe with the information we need.
    """
    pull_requests_json = _read_json_file(filepath)

    pull_requests = pd.DataFrame(
        [_get_data_from_pull_request(pr) for pr in pull_requests_json],
        columns=['id', 'author', 'created_time', 'merged_time', 'num_reviewers'])

    # Add a column for wall-clock time to complete
    pull_requests['ttl'] = pull_requests['merged_time'] - pull_requests['created_time']
    return pull_requests