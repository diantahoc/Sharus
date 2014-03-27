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

* `Sharush.GetHashInfo(string hash)` - for a single hash.
* `Sharush.GetHashesInfo(string[] hashes)` - for multiple hashes.

* `Sharush.GetHashInfoByURL(string url)` - to get hash information using a URL.
* `Sharush.GetHashesInfoByURL(string[] urls)` - for multiple URLs.

All the above methods return a `HashInfo` object or an array of it, which it will be detailled later.

* `Sharush.HashExist(string hash)` - return a boolean to indicate the existance of a hash.

* `Sharush.GetHashStatus(string hash)` - return a `KeyValuePair<HashStatus, HashInfo>` with the key as the hash status (an enum), and the `HashInfo` as the value. The value is always `null` except when the `HashStatus` is equal to `HashStatus.Done`. Please view the `Enums.cs` file for all possible values.

* `Sharush.GetHashesStatus(string[] hashes)` - same as `GetHashStatus`, except it returns a `Dictionary<string, KeyValuePair<HashStatus, HashInfo>>`. The Dictionary use the file hashes as the key, and a `KeyValuePair<HashStatus, HashInfo>` as the value.

* `Sharush.GetHashFlags(string hash)` - return an `IFlags` object. Currently it will only return a `VideoFlags` object, as the api only support video flags.

####The `HashInfo` object:

The HashInfo object is a container that provide all possible informations for a given hash.

This is the source of the `HashInfo` class:

```csharp
    public class HashInfo
    {
        public HashInfo(string hash)
        {
            this.Hash = hash;
        }

        public string Hash { get; private set; }

        public enum BlobType { Audio, Image, Video }

        public BlobType Type { get; set; }

        public double CompressionRatio { get; set; }

        public FileEntry[] Files { get; set; }

        public FileEntry[] ExtraFiles { get; set; }

        public string OriginalMimeType { get; set; }

        /// <summary>
        /// The original file that was uploaded, as-is.
        /// </summary>
        public string Original { get; set; }

        public IMetadata[] Metadata { get; set; }

        public IFlags Flags { get; set; }
    }
```

You may have noticed the `FileEntry` type, and also the `IMetadata` and `IFlags` interfaces.

####The `FileEntry` class

This class hold the acutal file data, such as the URL, the original file name, and the mimetype.

Basically, this JSON object that the API gives is converted to a `FileEntry`

```javascript
{
  "file": "/CPvuR5lRhmS0.mp4",
  "url": "https://mediacru.sh/CPvuR5lRhmS0.mp4",
  "type": "video/mp4"
}
```

The `FileEntry` public members are

```csharp
public string File { get; set; } // the file original name, as documented in the json api
public string MimeType { get; set; } // the mimetype
public string Url { get; set; } // the url
```

####The `IMetadata` interface

This is a dummy interface used to hold all possible types of metadata.

Currently, Sharus support 3 types of metadata:

`DimensionMetaData` - A class that have two members (Height and Width, both are `int`)
`AudioAvailableMetaData` - A dummy class, if it is inside the `IMetadata` array in the `HashEntry` class, then this hash have an audio.
`VideoAvailableMetaData` - Same as `AudioAvailableMetaData`, but to indicate video availability.

####The `IFlags` interface:

This is also a dummy interface. Currently the API support only video flags, but I like to keep things generic.

There is only one class that implements the `IFlags` interface, which is the `VideoFlags` class.

The `VideoFlags` has 3 public properties, `AutoPlay`, `Loop`, and `Mute`. All of these are a boolean (`bool`).


###Hash manipulation methods:

The following methods can manipulate hashes:

* `Sharush.SetHashFlags(string hash, KeyValuePair<string, object>[] flags)` - Update the `hash` flags specified in the `flags` parameter, and return a `KeyValuePair<FlagsManipulationResponse, IFlags>` with the resposne status as the key, and the new flags as the value. The value is always `null` except when the response status is `FlagsManipulationResponse.UpdateSuccessful`.

* `Sharush.DeleteHash(string hash)` - return a `DeleteHashStatus` enum, indicating the response status. Please see the `Enums.cs` file for all possible values.

####File Uploading


There is 2 ways to upload files, either from local files, or using URLs.

* `Sharush.UploadFile(string filename)` - Upload a file from a local file. The `filename` should be the full path of the file.
* `Sharush.UploadFileByURL(string url)` -  Upload a file using a URL. 

Both of these methods return `KeyValuePair<UploadFileStatus, KeyValuePair<string, HashInfo>>`, with the first KeyValuePair has the upload status as the key, and has another `KeyValuePair` as a value. The secondary `KeyValuePair` has the `file hash` as a key, and the `HashInfo` as a value. The `file hash` is always `null` except when the `UploadFileStatus` is `UploadFileStatus.Succes` or `UploadFileStatus.AlreadyUploaded`, and the `HashInfo` is always `null` except when the `UploadFileStatus` is `UploadFileStatus.AlreadyUploaded`.

Example:

```csharp
//The following example upload a local file and output the compression ratio achieved by MediaCrush.

var status = Sharush.UploadFile(@"D:\d\s.jpg");

if (status.Key == UploadFileStatus.AlreadyUploaded) 
{
  string file_hash = status.Value.Key;
  HashInfo hash_info = status.Value.Value;
                
  Console.WriteLine(hash_info.CompressionRatio);

}
else if (status.Key == UploadFileStatus.Success)
{
  string file_hash = status.Value.Key;

  // Get the file hash info in another call as the API json only return the hash, so status.Value.Value is null.
  HashInfo hash_info = Sharush.GetHashInfo(file_hash);

  Console.WriteLine(hash_info.CompressionRatio);
}
else
{
  Console.WriteLine(status.Key.ToString());
}
```

#Dependencies

Sharus use the Newtonsoft JSON library - http://json.codeplex.com/.

#License

Please see the LICENSE file.
