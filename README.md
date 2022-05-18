# LyricsWPF

A fast and reliable lyrics synchronisation tool for music lovers and singing enthusiasts.


# TODO:
Fix song change detection in listening party

To reduce memory and cpu usage:
Update Devbase Generic list with only an array. It should work(With functions names) like List<T>

 Found new bug with encoding Happens when

 New Song found -> LyricCollector searches for song and finds -> lyrics will be fetched -> (This is the bug)request to server(Accept-Encoding or Encoding to UTF-8) -> 
 Convert chars to readable text.
  How to fix?:
  Update Devbase RequestData-Object to handle accepted encodings
  Update Devbase Request-Class to interpret those Objects
  Update Devbase Request-Class to handle other encodings
  Use another library for web requests
  
Notes:

