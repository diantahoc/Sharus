Sharus is a .NET API Wrapper for MediaCrush-powered websites.


#Documentation

###Notes:

* Everything has been tested and confirmed to work. 

* All methods throw WebExceptions (if applicable), as there is no network errors handling.

###Changing the application url:

Changing the application url is simple as setting the value of `Sharus.ApplicationUrl` property. Sharus will use this value for all network I/O. Sharus will not validate the URL, and it must not end with a trailing `/`, such as `https://mediacru.sh`.

###Album Making

Making an album is simple as calling `Sharush.MakeAlbum(new string[] { "hash#1", "hash#2" })`. This method return a `KeyValuePair<AlbumMakeStatus, string>`, the response status is the key (an enum), and the value is the album hash.

The following program will attempt to make an album

```csharp
var album = Sharush.Sharush.MakeAlbum(new string[] { "hash#1", "hash#2" });

if (album.Key == Sharush.Enums.AlbumMakeStatus.Success) 
{
  Console.WriteLine("Album hash is {0}", album.Value);
}
else
{
  Console.WrilteLine("Cannot make the album for the following reason {0}", album.Key);
}
```

###Hash information retrival

Getting hash informations is pretty trivial. The following methods are used to get hash informations:

* `GetHashInfo` - for a single hash.
* `GetHashesInfo` - for multiple hashes. Accept an array of strings.
* `GetHashInfoByURL` - to get hash information using a URL.
* `GetHashesInfoByURL` - same as ``, but takes an array of strings.

All these return either a `HashInfo` object or an array of it.


###More docs to be added.


#Dependencies

Sharus use the Newtonsoft JSON library - http://json.codeplex.com/.

#License

Please see the LICENSE file.
