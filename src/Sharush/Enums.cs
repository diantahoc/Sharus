namespace Sharush.Enums
{
    public enum AlbumMakeStatus
    {
        /// <summary>
        /// The album was made correctly.
        /// HTTP Code: 200
        /// </summary>
        Success,
        /// <summary>
        /// At least one of the items in the list could not be found.
        /// HTTP Code: 404
        /// </summary>
        MissingHash,
        /// <summary>
        /// An attempt was made to create an album that was too large.
        /// HTTP Code: 413
        /// </summary>
        AlbumTooLarge,
        /// <summary>
        ///  At least one of the items in the list is not a File (i.e, you tried to create an album that cantains an album)
        ///  HTTP Code: 415
        /// </summary>
        InvalidHash,
        /// <summary>
        /// Server returned a null response
        /// </summary>
        NullResponse,
        /// <summary>
        /// Got a response, but with unkown status
        /// </summary>
        Unkown
    }

    public enum HashStatus
    {
        /// <summary>
        /// The file has been processed.
        /// </summary>
        Done,
        /// <summary>
        /// The file is still processing, but it is ready to be consumed by a web browser.
        /// </summary>
        Ready,
        /// <summary>
        /// The is in the processing queue.
        /// </summary>
        Pending,
        /// <summary>
        /// The file is currently being processed.
        /// </summary>
        Processing,
        /// <summary>
        /// A critical processing step finished early with an abnormal return code.
        /// </summary>
        Error,
        /// <summary>
        /// The file took too long to process.
        /// </summary>
        Timeout,
        /// <summary>
        /// MediaCrush does not support processing this media format.
        /// </summary>
        Unrecognised,
        /// <summary>
        /// The workers died unexpectedly. The client is advised to try again.
        /// </summary>
        InternalError,
        /// <summary>
        /// Unkown hash status
        /// </summary>
        Unknown
    }

    public enum FlagsManipulationResponse
    {
        /// <summary>
        /// The IP matches the stored hash and the flags have been updated.
        /// Http code: 200
        /// </summary>
        UpdateSuccessful,
        /// <summary>
        ///  The IP does not match the stored hash.
        ///  Http code: 401
        /// </summary>
        IpMismatch,
        /// <summary>
        /// There is no such hash.
        /// Http code: 404
        /// </summary>
        InvalidHash,
        /// <summary>
        /// One of the parameters passed to this endpoint is not recognised.
        /// Ensure your form data does not contain extraneous fields.
        /// Http code: 415
        /// </summary>
        InvalidParameters,
        EmptyResponse,
        Unknown
    }

    public enum DeleteHashStatus
    {
        /// <summary>
        /// The IP matches the stored hash and the file (if applicable) was deleted. 
        /// Http code: 200
        /// </summary>
        Success,
        /// <summary>
        /// The IP does not match the stored hash.
        /// Http code: 401
        /// </summary>
        IpMismatch,
        /// <summary>
        /// There is no such hash.
        /// Http code: 404
        /// </summary>
        NoSuchHash,
        NoResponse,
        Unknown
    }

    public enum UploadFileStatus
    {
        /// <summary>
        /// The file was uploaded correctly.
        /// Http code: 200
        /// </summary>
        Success,
        /// <summary>
        /// The URL is invalid.
        /// Http code: 400
        /// </summary>
        InvalidURL,
        /// <summary>
        /// The requested file does not exist.
        /// Http code: 404
        /// </summary>
        URLNotFound,
        /// <summary>
        /// The file was already uploaded.
        /// Http code: 409
        /// </summary>
        AlreadyUploaded,
        /// <summary>
        /// The file extension is not acceptable.
        /// Http code: 415
        /// </summary>
        UnacceptableFileExtension,
        /// <summary>
        /// The rate limit was exceeded. Enhance your calm.
        /// Http code: 420
        /// </summary>
        RateLimitExceeded,
        NoResponse,
        Unknown
    }
}
