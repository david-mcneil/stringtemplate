import re
import codecs
import warnings

def deprecated(func):
    def wrap(*args, **kwargs):
        warnings.warn(
            'Use of this method is deprecated, use property instead',
            DeprecationWarning,
            stacklevel=2
            )
        return func(*args, **kwargs)

    wrap.__name__ = func.__name__
    wrap.__doc__ = func.__doc__
    return wrap


def decodeFile(fp, filename, defaultEncoding='ascii'):
    """Autodetect file encoding and return a decoding filehandle.

    fp must be a filelike object that supports seeking. This function will
    look at the first to line to detect a charset declaration as described
    in PEP263:

    - if the file contains "coding: encoding" or "coding= encoding" on one
      of the first two lines, "encoding" is used as the file encoding. More
      specifically, this function looks for a regex "coding[:=]\s*([-\w.]+)"

    - if the file starts with a BOM, the encoding is 'utf-8'. If the file
      also contains a coding declaration, is must be 'utf-8'.

    """

    skip_bytes = 0
    has_bom = False

    prefix = fp.read(3)
    if prefix == '\xef\xbb\xbf':
        has_bom = True
        skip_bytes += 3
        prefix = ''

    shebang = '#!stringtemplate'
    prefix += fp.read(len(shebang) - len(prefix))
    if prefix == shebang:
        prefix += fp.readline()
        skip_bytes += len(prefix)

    encoding = None
    for _ in range(2):
        line = prefix + fp.readline()
        prefix = ""
        if not line:
            break

        m = re.search(r"coding[:=]\s*([-\w.]+)", line)
        if m is not None:
            encoding = m.group(1)
            break


    if has_bom:
        if encoding is not None and encoding.lower not in ('utf-8', 'utf8'):
            raise RuntimeError(
                "File %r has a BOM, but also a mismatching coding"
                "declaration for %r" % (filename, encoding))

        encoding = 'utf-8'

    fp.seek(skip_bytes) # skip BOM and or shebang when reading file

    if encoding is None:
        encoding = defaultEncoding
    return codecs.getreader(encoding)(fp)
