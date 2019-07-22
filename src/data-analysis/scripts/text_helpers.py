def iscamel(s: str) -> bool:
    """
    Return true if the string is in camel case format, false otherwise.
    """
    return s.isalnum() and s[0].isalpha() and s[0].islower()

def camel(s: str) -> str:
    """
    Convert an alphanumeric string to camel case.
    """
    if not s.isalnum():
        raise ValueError(f"'{s}' is not an alphanumeric string")
    return s[0].lower() + s[1:]

def remap_keys(key_func, d):
    """
    Create a new dictionary by passing the keys from an old dictionary through a function.
    """
    return dict((key_func(key), value) for key, value in d.items())
