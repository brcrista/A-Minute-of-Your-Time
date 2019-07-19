# A Minute of Your Time

![mockup-of-pull-request-notification](docs/images/hackathon-2019.png)

Having a pull request stuck in review for days on end is never fun.
Addressing all the feedback on a big PR can be frustrating for you, and it makes it harder for reviewers to provide helpful feedback on your changes.
Even when a large change is necessary, having an accurate estimate of how long it will take to get through review will help set expectations and reduce the risk of missing deadlines.

## TODO
* Make a tool to get data from the Azure Repos REST API
* Write a script to process this data to be consumed from Python
* Create a Python script that runs exploratory data analysis on the dataset, consumable as a Jupyter notebook
* Train machine learning models to predict the time to review a new pull request
* Automate the whole thing end-to-end