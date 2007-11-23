import warnings

def deprecated(func):
    def wrap(*args, **kwargs):
        warnings.warn(
            'Use of this method is deprecated, use property instead',
            DeprecationWarning,
            stacklevel=1
            )
        return wrap(*args, **kwargs)

    wrap.__name__ = func.__name__
    wrap.__doc__ = func.__doc__
    return wrap

