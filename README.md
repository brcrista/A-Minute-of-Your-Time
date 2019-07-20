# A Minute of Your Time

![mockup-of-pull-request-notification](docs/images/hackathon-2019.png)

Having a pull request stuck in review for days on end is never fun.
Addressing all the feedback on a big PR can be frustrating for you, and it makes it harder for reviewers to provide helpful feedback on your changes.
Even when a large change is necessary, having an accurate estimate of how long it will take to get through review will help set expectations and reduce the risk of missing deadlines.

## Instructions
### Fetching pull request data from your repo
Build the `FetchPullRequestData` tool:

```bash
cd src
dotnet build FetchPullRequestData
```

Navigate to the `bin` directory and run the tool:

```bash
dotnet FetchPullRequestData.dll --url https://dev.azure.com/my-org --project MyProject --pat ***** --repository RepositoryName --count 1000
```

The output will be in a directory called `./output`.
You can change this by passing the `--outdir` argument.

## TODO
- [x] Make a tool to get data from the Azure Repos REST API
- [ ] Write a script to process this data to be consumed from Python
- [ ] Create a Python script that runs exploratory data analysis on the dataset, consumable as a Jupyter notebook
- [ ] Train machine learning models to predict the time to review a new pull request
- [ ] Automate the whole thing end-to-end