def timedelta_to_hours(td):
    """
    Convert a `datetime.timedelta` to a fractional number of hours.
    """
    return 24 * td.days + td.seconds / 3600