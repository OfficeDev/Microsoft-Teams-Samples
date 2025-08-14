export type ApiError = {
  errorCode: ApiErrorCode;
  time: string;
  message: string | undefined;
};

export enum ApiErrorCode {
  /// Unknown error.
  Unknown = "Unknown",

  /// When a valid Auth token is not provided
  Unauthorized = "Unauthorized",

  /// A valid Auth token was provided, but it does not have permission to access the desired resource
  Forbidden = "Forbidden",

  /// When client performs invalid operation.
  InvalidOperation = "InvalidOperation",

  /// A provided argument is not valid
  ArgumentNotValid = "ArgumentNotValid",

  /// Consent required to make authenticated call to Graph
  AuthConsentRequired = "AuthConsentRequired",

  /// Graph Service Exception.
  GraphServiceException = "GraphServiceException",

  /// We were unable to find the requested Category.
  CategoryNotFound = "CategoryNotFound",

  /// We were unable to find the requested Item.
  ItemNotFound = "ItemNotFound",

  /// We were unable to find the requested Channel's Activity. This can occur if the bot is not registered in this Channel.
  ChannelActivityNotFound = "ChannelActivityNotFound",
}
