from ..text_helpers import camel, iscamel, remap_keys

def test_iscamel():
    assert iscamel('a')
    assert iscamel('a1')
    assert iscamel('helloWorld')
    assert iscamel('hell0World1')

    assert not iscamel('')
    assert not iscamel(' ')
    assert not iscamel('ABC')
    assert not iscamel('hello world')
    assert not iscamel('HelloWorld')
    assert not iscamel('1helloWorld')
    assert not iscamel('helloWorld!')

def test_camel():
    assert camel('a') == 'a'
    assert camel('A') == 'a'
    assert camel('HelloWorld123') == 'helloWorld123'
    assert camel('HELLOWORLD123') == 'hELLOWORLD123'

def test_remap_keys():
    d = {'a': 1, 'b': 2, 'c': 3}
    assert remap_keys(lambda x: x.upper(), d) == {'A': 1, 'B': 2, 'C': 3}