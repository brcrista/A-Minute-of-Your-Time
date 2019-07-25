import argparse
import datetime
import os
import pickle
import sys

import pandas as pd
from sklearn.linear_model import LinearRegression

sys.path.append('..')
from import_data import azure_repos

def save_model(model, filename):
    with open(filename, 'wb') as fd:
        pickle.dump(model, fd)

def load_model(filename):
    with open(filename, 'rb') as fd:
        return pickle.load(fd)

def train_model(pull_requests):
    X = pd.DataFrame()
    X['num_files_changed'] = pull_requests['num_files_changed'].fillna(0)
    # Assigned reviewers hack
    X['assigned_reviewers'] = pull_requests['num_reviewers'].apply(lambda x: x / 2)

    model = LinearRegression()
    model.fit(X=X, y=pull_requests['ttl'].apply(lambda td: td.days * 24 + td.seconds / 3600))
    return model

def display_timedelta(td):
    days = td.days
    hours = td.seconds // 3600
    return f"{days} day{'' if days == 1 else 's'}, {hours} hour{'' if hours == 1 else 's'}"

help = f'Usage: python {os.path.basename(__file__)} num_files_changed num_reviewers'

if __name__ == '__main__':
    if len(sys.argv) != 3:
        print(help)
        exit(1)

    try:
        num_files = int(sys.argv[1])
        num_reviewers = int(sys.argv[2])
    except ValueError:
        print(help)
        exit(1)

    # The directory of this script file
    this_dir = os.path.dirname(os.path.realpath(__file__))
    model_filename = os.path.basename(__file__).rstrip('.py') + '.model'
    model_path = os.path.join(this_dir, model_filename)

    if os.path.exists(model_path):
        model = load_model(model_path)
    else:
        pull_requests = azure_repos.load_data('../../data/pull-requests.json')
        model = train_model(pull_requests)
        save_model(model, model_path)

    estimate = model.predict([[num_files, num_reviewers]])[0]
    td = datetime.timedelta(hours=estimate)
    print(f"Estimated time for {num_files} files changed and {num_reviewers} reviewers: {display_timedelta(td)}")