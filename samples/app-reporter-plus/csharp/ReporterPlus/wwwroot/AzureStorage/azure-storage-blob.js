/*!
 * Azure Storage SDK for JavaScript - Blob, 10.5.0
 * Copyright (c) Microsoft and contributors. All rights reserved.
 */
(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ? factory(exports) :
    typeof define === 'function' && define.amd ? define(['exports'], factory) :
    (global = global || self, factory(global.azblob = {}));
}(this, function (exports) { 'use strict';

    // Copyright (c) Microsoft Corporation. All rights reserved.
    // Licensed under the MIT License. See License.txt in the project root for license information.
    /**
     * A collection of HttpHeaders that can be sent with a HTTP request.
     */
    function getHeaderKey(headerName) {
        return headerName.toLowerCase();
    }
    /**
     * A collection of HTTP header key/value pairs.
     */
    var HttpHeaders = /** @class */ (function () {
        function HttpHeaders(rawHeaders) {
            this._headersMap = {};
            if (rawHeaders) {
                for (var headerName in rawHeaders) {
                    this.set(headerName, rawHeaders[headerName]);
                }
            }
        }
        /**
         * Set a header in this collection with the provided name and value. The name is
         * case-insensitive.
         * @param headerName The name of the header to set. This value is case-insensitive.
         * @param headerValue The value of the header to set.
         */
        HttpHeaders.prototype.set = function (headerName, headerValue) {
            this._headersMap[getHeaderKey(headerName)] = { name: headerName, value: headerValue.toString() };
        };
        /**
         * Get the header value for the provided header name, or undefined if no header exists in this
         * collection with the provided name.
         * @param headerName The name of the header.
         */
        HttpHeaders.prototype.get = function (headerName) {
            var header = this._headersMap[getHeaderKey(headerName)];
            return !header ? undefined : header.value;
        };
        /**
         * Get whether or not this header collection contains a header entry for the provided header name.
         */
        HttpHeaders.prototype.contains = function (headerName) {
            return !!this._headersMap[getHeaderKey(headerName)];
        };
        /**
         * Remove the header with the provided headerName. Return whether or not the header existed and
         * was removed.
         * @param headerName The name of the header to remove.
         */
        HttpHeaders.prototype.remove = function (headerName) {
            var result = this.contains(headerName);
            delete this._headersMap[getHeaderKey(headerName)];
            return result;
        };
        /**
         * Get the headers that are contained this collection as an object.
         */
        HttpHeaders.prototype.rawHeaders = function () {
            var result = {};
            for (var headerKey in this._headersMap) {
                var header = this._headersMap[headerKey];
                result[header.name.toLowerCase()] = header.value;
            }
            return result;
        };
        /**
         * Get the headers that are contained in this collection as an array.
         */
        HttpHeaders.prototype.headersArray = function () {
            var headers = [];
            for (var headerKey in this._headersMap) {
                headers.push(this._headersMap[headerKey]);
            }
            return headers;
        };
        /**
         * Get the header names that are contained in this collection.
         */
        HttpHeaders.prototype.headerNames = function () {
            var headerNames = [];
            var headers = this.headersArray();
            for (var i = 0; i < headers.length; ++i) {
                headerNames.push(headers[i].name);
            }
            return headerNames;
        };
        /**
         * Get the header names that are contained in this collection.
         */
        HttpHeaders.prototype.headerValues = function () {
            var headerValues = [];
            var headers = this.headersArray();
            for (var i = 0; i < headers.length; ++i) {
                headerValues.push(headers[i].value);
            }
            return headerValues;
        };
        /**
         * Get the JSON object representation of this HTTP header collection.
         */
        HttpHeaders.prototype.toJson = function () {
            return this.rawHeaders();
        };
        /**
         * Get the string representation of this HTTP header collection.
         */
        HttpHeaders.prototype.toString = function () {
            return JSON.stringify(this.toJson());
        };
        /**
         * Create a deep clone/copy of this HttpHeaders collection.
         */
        HttpHeaders.prototype.clone = function () {
            return new HttpHeaders(this.rawHeaders());
        };
        return HttpHeaders;
    }());

    // Copyright (c) Microsoft Corporation. All rights reserved.
    /**
     * Encodes a byte array in base64 format.
     * @param value the Uint8Aray to encode
     */
    function encodeByteArray(value) {
        var str = "";
        for (var i = 0; i < value.length; i++) {
            str += String.fromCharCode(value[i]);
        }
        return btoa(str);
    }
    /**
     * Decodes a base64 string into a byte array.
     * @param value the base64 string to decode
     */
    function decodeString(value) {
        var byteString = atob(value);
        var arr = new Uint8Array(byteString.length);
        for (var i = 0; i < byteString.length; i++) {
            arr[i] = byteString.charCodeAt(i);
        }
        return arr;
    }

    function createCommonjsModule(fn, module) {
    	return module = { exports: {} }, fn(module, module.exports), module.exports;
    }

    var rngBrowser = createCommonjsModule(function (module) {
    // Unique ID creation requires a high quality random # generator.  In the
    // browser this is a little complicated due to unknown quality of Math.random()
    // and inconsistent support for the `crypto` API.  We do the best we can via
    // feature-detection

    // getRandomValues needs to be invoked in a context where "this" is a Crypto
    // implementation. Also, find the complete implementation of crypto on IE11.
    var getRandomValues = (typeof(crypto) != 'undefined' && crypto.getRandomValues && crypto.getRandomValues.bind(crypto)) ||
                          (typeof(msCrypto) != 'undefined' && typeof window.msCrypto.getRandomValues == 'function' && msCrypto.getRandomValues.bind(msCrypto));

    if (getRandomValues) {
      // WHATWG crypto RNG - http://wiki.whatwg.org/wiki/Crypto
      var rnds8 = new Uint8Array(16); // eslint-disable-line no-undef

      module.exports = function whatwgRNG() {
        getRandomValues(rnds8);
        return rnds8;
      };
    } else {
      // Math.random()-based (RNG)
      //
      // If all else fails, use Math.random().  It's fast, but is of unspecified
      // quality.
      var rnds = new Array(16);

      module.exports = function mathRNG() {
        for (var i = 0, r; i < 16; i++) {
          if ((i & 0x03) === 0) r = Math.random() * 0x100000000;
          rnds[i] = r >>> ((i & 0x03) << 3) & 0xff;
        }

        return rnds;
      };
    }
    });

    /**
     * Convert array of 16 byte values to UUID string format of the form:
     * XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX
     */
    var byteToHex = [];
    for (var i = 0; i < 256; ++i) {
      byteToHex[i] = (i + 0x100).toString(16).substr(1);
    }

    function bytesToUuid(buf, offset) {
      var i = offset || 0;
      var bth = byteToHex;
      // join used to fix memory issue caused by concatenation: https://bugs.chromium.org/p/v8/issues/detail?id=3175#c4
      return ([bth[buf[i++]], bth[buf[i++]], 
    	bth[buf[i++]], bth[buf[i++]], '-',
    	bth[buf[i++]], bth[buf[i++]], '-',
    	bth[buf[i++]], bth[buf[i++]], '-',
    	bth[buf[i++]], bth[buf[i++]], '-',
    	bth[buf[i++]], bth[buf[i++]],
    	bth[buf[i++]], bth[buf[i++]],
    	bth[buf[i++]], bth[buf[i++]]]).join('');
    }

    var bytesToUuid_1 = bytesToUuid;

    function v4(options, buf, offset) {
      var i = buf && offset || 0;

      if (typeof(options) == 'string') {
        buf = options === 'binary' ? new Array(16) : null;
        options = null;
      }
      options = options || {};

      var rnds = options.random || (options.rng || rngBrowser)();

      // Per 4.4, set bits for version and `clock_seq_hi_and_reserved`
      rnds[6] = (rnds[6] & 0x0f) | 0x40;
      rnds[8] = (rnds[8] & 0x3f) | 0x80;

      // Copy bytes to buffer, if provided
      if (buf) {
        for (var ii = 0; ii < 16; ++ii) {
          buf[i + ii] = rnds[ii];
        }
      }

      return buf || bytesToUuid_1(rnds);
    }

    var v4_1 = v4;

    // Copyright (c) Microsoft Corporation. All rights reserved.
    // Licensed under the MIT License. See License.txt in the project root for license information.
    var Constants = {
        /**
         * The ms-rest version
         * @const
         * @type {string}
         */
        msRestVersion: "2.0.4",
        /**
         * Specifies HTTP.
         *
         * @const
         * @type {string}
         */
        HTTP: "http:",
        /**
         * Specifies HTTPS.
         *
         * @const
         * @type {string}
         */
        HTTPS: "https:",
        /**
         * Specifies HTTP Proxy.
         *
         * @const
         * @type {string}
         */
        HTTP_PROXY: "HTTP_PROXY",
        /**
         * Specifies HTTPS Proxy.
         *
         * @const
         * @type {string}
         */
        HTTPS_PROXY: "HTTPS_PROXY",
        HttpConstants: {
            /**
             * Http Verbs
             *
             * @const
             * @enum {string}
             */
            HttpVerbs: {
                PUT: "PUT",
                GET: "GET",
                DELETE: "DELETE",
                POST: "POST",
                MERGE: "MERGE",
                HEAD: "HEAD",
                PATCH: "PATCH"
            },
            StatusCodes: {
                TooManyRequests: 429
            }
        },
        /**
         * Defines constants for use with HTTP headers.
         */
        HeaderConstants: {
            /**
             * The Authorization header.
             *
             * @const
             * @type {string}
             */
            AUTHORIZATION: "authorization",
            AUTHORIZATION_SCHEME: "Bearer",
            /**
             * The Retry-After response-header field can be used with a 503 (Service
             * Unavailable) or 349 (Too Many Requests) responses to indicate how long
             * the service is expected to be unavailable to the requesting client.
             *
             * @const
             * @type {string}
             */
            RETRY_AFTER: "Retry-After",
            /**
             * The UserAgent header.
             *
             * @const
             * @type {string}
             */
            USER_AGENT: "User-Agent"
        }
    };

    // Copyright (c) Microsoft Corporation. All rights reserved.
    /**
     * A constant that indicates whether the environment is node.js or browser based.
     */
    var isNode = (typeof process !== "undefined") && !!process.version && !!process.versions && !!process.versions.node;
    /**
     * Returns a stripped version of the Http Response which only contains body,
     * headers and the status.
     *
     * @param {HttpOperationResponse} response The Http Response
     *
     * @return {object} The stripped version of Http Response.
     */
    function stripResponse(response) {
        var strippedResponse = {};
        strippedResponse.body = response.bodyAsText;
        strippedResponse.headers = response.headers;
        strippedResponse.status = response.status;
        return strippedResponse;
    }
    /**
     * Returns a stripped version of the Http Request that does not contain the
     * Authorization header.
     *
     * @param {WebResource} request The Http Request object
     *
     * @return {WebResource} The stripped version of Http Request.
     */
    function stripRequest(request) {
        var strippedRequest = request.clone();
        if (strippedRequest.headers) {
            strippedRequest.headers.remove("authorization");
        }
        return strippedRequest;
    }
    /**
     * Validates the given uuid as a string
     *
     * @param {string} uuid The uuid as a string that needs to be validated
     *
     * @return {boolean} True if the uuid is valid; false otherwise.
     */
    function isValidUuid(uuid) {
        var validUuidRegex = new RegExp("^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", "ig");
        return validUuidRegex.test(uuid);
    }
    /**
     * Generated UUID
     *
     * @return {string} RFC4122 v4 UUID.
     */
    function generateUuid() {
        return v4_1();
    }
    /**
     * A wrapper for setTimeout that resolves a promise after t milliseconds.
     * @param {number} t The number of milliseconds to be delayed.
     * @param {T} value The value to be resolved with after a timeout of t milliseconds.
     * @returns {Promise<T>} Resolved promise
     */
    function delay(t, value) {
        return new Promise(function (resolve) { return setTimeout(function () { return resolve(value); }, t); });
    }
    function prepareXMLRootList(obj, elementName) {
        var _a;
        if (!Array.isArray(obj)) {
            obj = [obj];
        }
        return _a = {}, _a[elementName] = obj, _a;
    }
    var validateISODuration = /^(-|\+)?P(?:([-+]?[0-9,.]*)Y)?(?:([-+]?[0-9,.]*)M)?(?:([-+]?[0-9,.]*)W)?(?:([-+]?[0-9,.]*)D)?(?:T(?:([-+]?[0-9,.]*)H)?(?:([-+]?[0-9,.]*)M)?(?:([-+]?[0-9,.]*)S)?)?$/;
    /**
     * Indicates whether the given string is in ISO 8601 format.
     * @param {string} value The value to be validated for ISO 8601 duration format.
     * @return {boolean} `true` if valid, `false` otherwise.
     */
    function isDuration(value) {
        return validateISODuration.test(value);
    }
    /**
     * Replace all of the instances of searchValue in value with the provided replaceValue.
     * @param {string | undefined} value The value to search and replace in.
     * @param {string} searchValue The value to search for in the value argument.
     * @param {string} replaceValue The value to replace searchValue with in the value argument.
     * @returns {string | undefined} The value where each instance of searchValue was replaced with replacedValue.
     */
    function replaceAll(value, searchValue, replaceValue) {
        return !value || !searchValue ? value : value.split(searchValue).join(replaceValue || "");
    }
    /**
     * Determines whether the given enity is a basic/primitive type
     * (string, number, boolean, null, undefined).
     * @param value Any entity
     * @return boolean - true is it is primitive type, false otherwise.
     */
    function isPrimitiveType(value) {
        return (typeof value !== "object" && typeof value !== "function") || value === null;
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    var Serializer = /** @class */ (function () {
        function Serializer(modelMappers, isXML) {
            if (modelMappers === void 0) { modelMappers = {}; }
            this.modelMappers = modelMappers;
            this.isXML = isXML;
        }
        Serializer.prototype.validateConstraints = function (mapper, value, objectName) {
            var failValidation = function (constraintName, constraintValue) {
                throw new Error("\"" + objectName + "\" with value \"" + value + "\" should satisfy the constraint \"" + constraintName + "\": " + constraintValue + ".");
            };
            if (mapper.constraints && (value != undefined)) {
                var _a = mapper.constraints, ExclusiveMaximum = _a.ExclusiveMaximum, ExclusiveMinimum = _a.ExclusiveMinimum, InclusiveMaximum = _a.InclusiveMaximum, InclusiveMinimum = _a.InclusiveMinimum, MaxItems = _a.MaxItems, MaxLength = _a.MaxLength, MinItems = _a.MinItems, MinLength = _a.MinLength, MultipleOf = _a.MultipleOf, Pattern = _a.Pattern, UniqueItems = _a.UniqueItems;
                if (ExclusiveMaximum != undefined && value >= ExclusiveMaximum) {
                    failValidation("ExclusiveMaximum", ExclusiveMaximum);
                }
                if (ExclusiveMinimum != undefined && value <= ExclusiveMinimum) {
                    failValidation("ExclusiveMinimum", ExclusiveMinimum);
                }
                if (InclusiveMaximum != undefined && value > InclusiveMaximum) {
                    failValidation("InclusiveMaximum", InclusiveMaximum);
                }
                if (InclusiveMinimum != undefined && value < InclusiveMinimum) {
                    failValidation("InclusiveMinimum", InclusiveMinimum);
                }
                if (MaxItems != undefined && value.length > MaxItems) {
                    failValidation("MaxItems", MaxItems);
                }
                if (MaxLength != undefined && value.length > MaxLength) {
                    failValidation("MaxLength", MaxLength);
                }
                if (MinItems != undefined && value.length < MinItems) {
                    failValidation("MinItems", MinItems);
                }
                if (MinLength != undefined && value.length < MinLength) {
                    failValidation("MinLength", MinLength);
                }
                if (MultipleOf != undefined && value % MultipleOf !== 0) {
                    failValidation("MultipleOf", MultipleOf);
                }
                if (Pattern) {
                    var pattern = typeof Pattern === "string" ? new RegExp(Pattern) : Pattern;
                    if ((typeof value !== "string") || (value.match(pattern) === null)) {
                        failValidation("Pattern", Pattern);
                    }
                }
                if (UniqueItems && value.some(function (item, i, ar) { return ar.indexOf(item) !== i; })) {
                    failValidation("UniqueItems", UniqueItems);
                }
            }
        };
        /**
         * Serialize the given object based on its metadata defined in the mapper
         *
         * @param {Mapper} mapper The mapper which defines the metadata of the serializable object
         *
         * @param {object|string|Array|number|boolean|Date|stream} object A valid Javascript object to be serialized
         *
         * @param {string} objectName Name of the serialized object
         *
         * @returns {object|string|Array|number|boolean|Date|stream} A valid serialized Javascript object
         */
        Serializer.prototype.serialize = function (mapper, object, objectName) {
            var payload = {};
            var mapperType = mapper.type.name;
            if (!objectName) {
                objectName = mapper.serializedName;
            }
            if (mapperType.match(/^Sequence$/ig) !== null) {
                payload = [];
            }
            if (mapper.isConstant) {
                object = mapper.defaultValue;
            }
            // This table of allowed values should help explain
            // the mapper.required and mapper.nullable properties.
            // X means "neither undefined or null are allowed".
            //           || required
            //           || true      | false
            //  nullable || ==========================
            //      true || null      | undefined/null
            //     false || X         | undefined
            // undefined || X         | undefined/null
            var required = mapper.required, nullable = mapper.nullable;
            if (required && nullable && object === undefined) {
                throw new Error(objectName + " cannot be undefined.");
            }
            if (required && !nullable && object == undefined) {
                throw new Error(objectName + " cannot be null or undefined.");
            }
            if (!required && nullable === false && object === null) {
                throw new Error(objectName + " cannot be null.");
            }
            if (object == undefined) {
                payload = object;
            }
            else {
                // Validate Constraints if any
                this.validateConstraints(mapper, object, objectName);
                if (mapperType.match(/^any$/ig) !== null) {
                    payload = object;
                }
                else if (mapperType.match(/^(Number|String|Boolean|Object|Stream|Uuid)$/ig) !== null) {
                    payload = serializeBasicTypes(mapperType, objectName, object);
                }
                else if (mapperType.match(/^Enum$/ig) !== null) {
                    var enumMapper = mapper;
                    payload = serializeEnumType(objectName, enumMapper.type.allowedValues, object);
                }
                else if (mapperType.match(/^(Date|DateTime|TimeSpan|DateTimeRfc1123|UnixTime)$/ig) !== null) {
                    payload = serializeDateTypes(mapperType, object, objectName);
                }
                else if (mapperType.match(/^ByteArray$/ig) !== null) {
                    payload = serializeByteArrayType(objectName, object);
                }
                else if (mapperType.match(/^Base64Url$/ig) !== null) {
                    payload = serializeBase64UrlType(objectName, object);
                }
                else if (mapperType.match(/^Sequence$/ig) !== null) {
                    payload = serializeSequenceType(this, mapper, object, objectName);
                }
                else if (mapperType.match(/^Dictionary$/ig) !== null) {
                    payload = serializeDictionaryType(this, mapper, object, objectName);
                }
                else if (mapperType.match(/^Composite$/ig) !== null) {
                    payload = serializeCompositeType(this, mapper, object, objectName);
                }
            }
            return payload;
        };
        /**
         * Deserialize the given object based on its metadata defined in the mapper
         *
         * @param {object} mapper The mapper which defines the metadata of the serializable object
         *
         * @param {object|string|Array|number|boolean|Date|stream} responseBody A valid Javascript entity to be deserialized
         *
         * @param {string} objectName Name of the deserialized object
         *
         * @returns {object|string|Array|number|boolean|Date|stream} A valid deserialized Javascript object
         */
        Serializer.prototype.deserialize = function (mapper, responseBody, objectName) {
            if (responseBody == undefined) {
                if (this.isXML && mapper.type.name === "Sequence" && !mapper.xmlIsWrapped) {
                    // Edge case for empty XML non-wrapped lists. xml2js can't distinguish
                    // between the list being empty versus being missing,
                    // so let's do the more user-friendly thing and return an empty list.
                    responseBody = [];
                }
                // specifically check for undefined as default value can be a falsey value `0, "", false, null`
                if (mapper.defaultValue !== undefined) {
                    responseBody = mapper.defaultValue;
                }
                return responseBody;
            }
            var payload;
            var mapperType = mapper.type.name;
            if (!objectName) {
                objectName = mapper.serializedName;
            }
            if (mapperType.match(/^Composite$/ig) !== null) {
                payload = deserializeCompositeType(this, mapper, responseBody, objectName);
            }
            else {
                if (this.isXML) {
                    /**
                     * If the mapper specifies this as a non-composite type value but the responseBody contains
                     * both header ("$") and body ("_") properties, then just reduce the responseBody value to
                     * the body ("_") property.
                     */
                    if (responseBody["$"] != undefined && responseBody["_"] != undefined) {
                        responseBody = responseBody["_"];
                    }
                }
                if (mapperType.match(/^Number$/ig) !== null) {
                    payload = parseFloat(responseBody);
                    if (isNaN(payload)) {
                        payload = responseBody;
                    }
                }
                else if (mapperType.match(/^Boolean$/ig) !== null) {
                    if (responseBody === "true") {
                        payload = true;
                    }
                    else if (responseBody === "false") {
                        payload = false;
                    }
                    else {
                        payload = responseBody;
                    }
                }
                else if (mapperType.match(/^(String|Enum|Object|Stream|Uuid|TimeSpan|any)$/ig) !== null) {
                    payload = responseBody;
                }
                else if (mapperType.match(/^(Date|DateTime|DateTimeRfc1123)$/ig) !== null) {
                    payload = new Date(responseBody);
                }
                else if (mapperType.match(/^UnixTime$/ig) !== null) {
                    payload = unixTimeToDate(responseBody);
                }
                else if (mapperType.match(/^ByteArray$/ig) !== null) {
                    payload = decodeString(responseBody);
                }
                else if (mapperType.match(/^Base64Url$/ig) !== null) {
                    payload = base64UrlToByteArray(responseBody);
                }
                else if (mapperType.match(/^Sequence$/ig) !== null) {
                    payload = deserializeSequenceType(this, mapper, responseBody, objectName);
                }
                else if (mapperType.match(/^Dictionary$/ig) !== null) {
                    payload = deserializeDictionaryType(this, mapper, responseBody, objectName);
                }
            }
            if (mapper.isConstant) {
                payload = mapper.defaultValue;
            }
            return payload;
        };
        return Serializer;
    }());
    function trimEnd(str, ch) {
        var len = str.length;
        while ((len - 1) >= 0 && str[len - 1] === ch) {
            --len;
        }
        return str.substr(0, len);
    }
    function bufferToBase64Url(buffer) {
        if (!buffer) {
            return undefined;
        }
        if (!(buffer instanceof Uint8Array)) {
            throw new Error("Please provide an input of type Uint8Array for converting to Base64Url.");
        }
        // Uint8Array to Base64.
        var str = encodeByteArray(buffer);
        // Base64 to Base64Url.
        return trimEnd(str, "=").replace(/\+/g, "-").replace(/\//g, "_");
    }
    function base64UrlToByteArray(str) {
        if (!str) {
            return undefined;
        }
        if (str && typeof str.valueOf() !== "string") {
            throw new Error("Please provide an input of type string for converting to Uint8Array");
        }
        // Base64Url to Base64.
        str = str.replace(/\-/g, "+").replace(/\_/g, "/");
        // Base64 to Uint8Array.
        return decodeString(str);
    }
    function splitSerializeName(prop) {
        var classes = [];
        var partialclass = "";
        if (prop) {
            var subwords = prop.split(".");
            for (var _i = 0, subwords_1 = subwords; _i < subwords_1.length; _i++) {
                var item = subwords_1[_i];
                if (item.charAt(item.length - 1) === "\\") {
                    partialclass += item.substr(0, item.length - 1) + ".";
                }
                else {
                    partialclass += item;
                    classes.push(partialclass);
                    partialclass = "";
                }
            }
        }
        return classes;
    }
    function dateToUnixTime(d) {
        if (!d) {
            return undefined;
        }
        if (typeof d.valueOf() === "string") {
            d = new Date(d);
        }
        return Math.floor(d.getTime() / 1000);
    }
    function unixTimeToDate(n) {
        if (!n) {
            return undefined;
        }
        return new Date(n * 1000);
    }
    function serializeBasicTypes(typeName, objectName, value) {
        if (value !== null && value !== undefined) {
            if (typeName.match(/^Number$/ig) !== null) {
                if (typeof value !== "number") {
                    throw new Error(objectName + " with value " + value + " must be of type number.");
                }
            }
            else if (typeName.match(/^String$/ig) !== null) {
                if (typeof value.valueOf() !== "string") {
                    throw new Error(objectName + " with value \"" + value + "\" must be of type string.");
                }
            }
            else if (typeName.match(/^Uuid$/ig) !== null) {
                if (!(typeof value.valueOf() === "string" && isValidUuid(value))) {
                    throw new Error(objectName + " with value \"" + value + "\" must be of type string and a valid uuid.");
                }
            }
            else if (typeName.match(/^Boolean$/ig) !== null) {
                if (typeof value !== "boolean") {
                    throw new Error(objectName + " with value " + value + " must be of type boolean.");
                }
            }
            else if (typeName.match(/^Stream$/ig) !== null) {
                var objectType = typeof value;
                if (objectType !== "string" &&
                    objectType !== "function" &&
                    !(value instanceof ArrayBuffer) &&
                    !ArrayBuffer.isView(value) &&
                    !(typeof Blob === "function" && value instanceof Blob)) {
                    throw new Error(objectName + " must be a string, Blob, ArrayBuffer, ArrayBufferView, or a function returning NodeJS.ReadableStream.");
                }
            }
        }
        return value;
    }
    function serializeEnumType(objectName, allowedValues, value) {
        if (!allowedValues) {
            throw new Error("Please provide a set of allowedValues to validate " + objectName + " as an Enum Type.");
        }
        var isPresent = allowedValues.some(function (item) {
            if (typeof item.valueOf() === "string") {
                return item.toLowerCase() === value.toLowerCase();
            }
            return item === value;
        });
        if (!isPresent) {
            throw new Error(value + " is not a valid value for " + objectName + ". The valid values are: " + JSON.stringify(allowedValues) + ".");
        }
        return value;
    }
    function serializeByteArrayType(objectName, value) {
        if (value != undefined) {
            if (!(value instanceof Uint8Array)) {
                throw new Error(objectName + " must be of type Uint8Array.");
            }
            value = encodeByteArray(value);
        }
        return value;
    }
    function serializeBase64UrlType(objectName, value) {
        if (value != undefined) {
            if (!(value instanceof Uint8Array)) {
                throw new Error(objectName + " must be of type Uint8Array.");
            }
            value = bufferToBase64Url(value);
        }
        return value;
    }
    function serializeDateTypes(typeName, value, objectName) {
        if (value != undefined) {
            if (typeName.match(/^Date$/ig) !== null) {
                if (!(value instanceof Date ||
                    (typeof value.valueOf() === "string" && !isNaN(Date.parse(value))))) {
                    throw new Error(objectName + " must be an instanceof Date or a string in ISO8601 format.");
                }
                value = (value instanceof Date) ? value.toISOString().substring(0, 10) : new Date(value).toISOString().substring(0, 10);
            }
            else if (typeName.match(/^DateTime$/ig) !== null) {
                if (!(value instanceof Date ||
                    (typeof value.valueOf() === "string" && !isNaN(Date.parse(value))))) {
                    throw new Error(objectName + " must be an instanceof Date or a string in ISO8601 format.");
                }
                value = (value instanceof Date) ? value.toISOString() : new Date(value).toISOString();
            }
            else if (typeName.match(/^DateTimeRfc1123$/ig) !== null) {
                if (!(value instanceof Date ||
                    (typeof value.valueOf() === "string" && !isNaN(Date.parse(value))))) {
                    throw new Error(objectName + " must be an instanceof Date or a string in RFC-1123 format.");
                }
                value = (value instanceof Date) ? value.toUTCString() : new Date(value).toUTCString();
            }
            else if (typeName.match(/^UnixTime$/ig) !== null) {
                if (!(value instanceof Date ||
                    (typeof value.valueOf() === "string" && !isNaN(Date.parse(value))))) {
                    throw new Error(objectName + " must be an instanceof Date or a string in RFC-1123/ISO8601 format " +
                        "for it to be serialized in UnixTime/Epoch format.");
                }
                value = dateToUnixTime(value);
            }
            else if (typeName.match(/^TimeSpan$/ig) !== null) {
                if (!isDuration(value)) {
                    throw new Error(objectName + " must be a string in ISO 8601 format. Instead was \"" + value + "\".");
                }
                value = value;
            }
        }
        return value;
    }
    function serializeSequenceType(serializer, mapper, object, objectName) {
        if (!Array.isArray(object)) {
            throw new Error(objectName + " must be of type Array.");
        }
        var elementType = mapper.type.element;
        if (!elementType || typeof elementType !== "object") {
            throw new Error("element\" metadata for an Array must be defined in the " +
                ("mapper and it must of type \"object\" in " + objectName + "."));
        }
        var tempArray = [];
        for (var i = 0; i < object.length; i++) {
            tempArray[i] = serializer.serialize(elementType, object[i], objectName);
        }
        return tempArray;
    }
    function serializeDictionaryType(serializer, mapper, object, objectName) {
        if (typeof object !== "object") {
            throw new Error(objectName + " must be of type object.");
        }
        var valueType = mapper.type.value;
        if (!valueType || typeof valueType !== "object") {
            throw new Error("\"value\" metadata for a Dictionary must be defined in the " +
                ("mapper and it must of type \"object\" in " + objectName + "."));
        }
        var tempDictionary = {};
        for (var _i = 0, _a = Object.keys(object); _i < _a.length; _i++) {
            var key = _a[_i];
            tempDictionary[key] = serializer.serialize(valueType, object[key], objectName + "." + key);
        }
        return tempDictionary;
    }
    /**
     * Resolves a composite mapper's modelProperties.
     * @param serializer the serializer containing the entire set of mappers
     * @param mapper the composite mapper to resolve
     */
    function resolveModelProperties(serializer, mapper, objectName) {
        var modelProps = mapper.type.modelProperties;
        if (!modelProps) {
            var className = mapper.type.className;
            if (!className) {
                throw new Error("Class name for model \"" + objectName + "\" is not provided in the mapper \"" + JSON.stringify(mapper, undefined, 2) + "\".");
            }
            var modelMapper = serializer.modelMappers[className];
            if (!modelMapper) {
                throw new Error("mapper() cannot be null or undefined for model \"" + className + "\".");
            }
            modelProps = modelMapper.type.modelProperties;
            if (!modelProps) {
                throw new Error("modelProperties cannot be null or undefined in the " +
                    ("mapper \"" + JSON.stringify(modelMapper) + "\" of type \"" + className + "\" for object \"" + objectName + "\"."));
            }
        }
        return modelProps;
    }
    function serializeCompositeType(serializer, mapper, object, objectName) {
        var _a;
        if (getPolymorphicDiscriminatorRecursively(serializer, mapper)) {
            mapper = getPolymorphicMapper(serializer, mapper, object, "clientName");
        }
        if (object != undefined) {
            var payload = {};
            var modelProps = resolveModelProperties(serializer, mapper, objectName);
            for (var _i = 0, _b = Object.keys(modelProps); _i < _b.length; _i++) {
                var key = _b[_i];
                var propertyMapper = modelProps[key];
                if (propertyMapper.readOnly) {
                    continue;
                }
                var propName = void 0;
                var parentObject = payload;
                if (serializer.isXML) {
                    if (propertyMapper.xmlIsWrapped) {
                        propName = propertyMapper.xmlName;
                    }
                    else {
                        propName = propertyMapper.xmlElementName || propertyMapper.xmlName;
                    }
                }
                else {
                    var paths = splitSerializeName(propertyMapper.serializedName);
                    propName = paths.pop();
                    for (var _c = 0, paths_1 = paths; _c < paths_1.length; _c++) {
                        var pathName = paths_1[_c];
                        var childObject = parentObject[pathName];
                        if ((childObject == undefined) && (object[key] != undefined)) {
                            parentObject[pathName] = {};
                        }
                        parentObject = parentObject[pathName];
                    }
                }
                if (parentObject != undefined) {
                    var propertyObjectName = propertyMapper.serializedName !== ""
                        ? objectName + "." + propertyMapper.serializedName
                        : objectName;
                    var toSerialize = object[key];
                    var polymorphicDiscriminator = getPolymorphicDiscriminatorRecursively(serializer, mapper);
                    if (polymorphicDiscriminator && polymorphicDiscriminator.clientName === key && toSerialize == undefined) {
                        toSerialize = mapper.serializedName;
                    }
                    var serializedValue = serializer.serialize(propertyMapper, toSerialize, propertyObjectName);
                    if (serializedValue !== undefined && propName != undefined) {
                        if (propertyMapper.xmlIsAttribute) {
                            // $ is the key attributes are kept under in xml2js.
                            // This keeps things simple while preventing name collision
                            // with names in user documents.
                            parentObject.$ = parentObject.$ || {};
                            parentObject.$[propName] = serializedValue;
                        }
                        else if (propertyMapper.xmlIsWrapped) {
                            parentObject[propName] = (_a = {}, _a[propertyMapper.xmlElementName] = serializedValue, _a);
                        }
                        else {
                            parentObject[propName] = serializedValue;
                        }
                    }
                }
            }
            var additionalPropertiesMapper = mapper.type.additionalProperties;
            if (additionalPropertiesMapper) {
                var propNames = Object.keys(modelProps);
                var _loop_1 = function (clientPropName) {
                    var isAdditionalProperty = propNames.every(function (pn) { return pn !== clientPropName; });
                    if (isAdditionalProperty) {
                        payload[clientPropName] = serializer.serialize(additionalPropertiesMapper, object[clientPropName], objectName + '["' + clientPropName + '"]');
                    }
                };
                for (var clientPropName in object) {
                    _loop_1(clientPropName);
                }
            }
            return payload;
        }
        return object;
    }
    function isSpecialXmlProperty(propertyName) {
        return ["$", "_"].includes(propertyName);
    }
    function deserializeCompositeType(serializer, mapper, responseBody, objectName) {
        if (getPolymorphicDiscriminatorRecursively(serializer, mapper)) {
            mapper = getPolymorphicMapper(serializer, mapper, responseBody, "serializedName");
        }
        var modelProps = resolveModelProperties(serializer, mapper, objectName);
        var instance = {};
        var handledPropertyNames = [];
        for (var _i = 0, _a = Object.keys(modelProps); _i < _a.length; _i++) {
            var key = _a[_i];
            var propertyMapper = modelProps[key];
            var paths = splitSerializeName(modelProps[key].serializedName);
            handledPropertyNames.push(paths[0]);
            var serializedName = propertyMapper.serializedName, xmlName = propertyMapper.xmlName, xmlElementName = propertyMapper.xmlElementName;
            var propertyObjectName = objectName;
            if (serializedName !== "" && serializedName !== undefined) {
                propertyObjectName = objectName + "." + serializedName;
            }
            var headerCollectionPrefix = propertyMapper.headerCollectionPrefix;
            if (headerCollectionPrefix) {
                var dictionary = {};
                for (var _b = 0, _c = Object.keys(responseBody); _b < _c.length; _b++) {
                    var headerKey = _c[_b];
                    if (headerKey.startsWith(headerCollectionPrefix)) {
                        dictionary[headerKey.substring(headerCollectionPrefix.length)] = serializer.deserialize(propertyMapper.type.value, responseBody[headerKey], propertyObjectName);
                    }
                    handledPropertyNames.push(headerKey);
                }
                instance[key] = dictionary;
            }
            else if (serializer.isXML) {
                if (propertyMapper.xmlIsAttribute && responseBody.$) {
                    instance[key] = serializer.deserialize(propertyMapper, responseBody.$[xmlName], propertyObjectName);
                }
                else {
                    var propertyName = xmlElementName || xmlName || serializedName;
                    var unwrappedProperty = responseBody[propertyName];
                    if (propertyMapper.xmlIsWrapped) {
                        unwrappedProperty = responseBody[xmlName];
                        unwrappedProperty = unwrappedProperty && unwrappedProperty[xmlElementName];
                        var isEmptyWrappedList = unwrappedProperty === undefined;
                        if (isEmptyWrappedList) {
                            unwrappedProperty = [];
                        }
                    }
                    instance[key] = serializer.deserialize(propertyMapper, unwrappedProperty, propertyObjectName);
                }
            }
            else {
                // deserialize the property if it is present in the provided responseBody instance
                var propertyInstance = void 0;
                var res = responseBody;
                // traversing the object step by step.
                for (var _d = 0, paths_2 = paths; _d < paths_2.length; _d++) {
                    var item = paths_2[_d];
                    if (!res)
                        break;
                    res = res[item];
                }
                propertyInstance = res;
                var polymorphicDiscriminator = mapper.type.polymorphicDiscriminator;
                // checking that the model property name (key)(ex: "fishtype") and the
                // clientName of the polymorphicDiscriminator {metadata} (ex: "fishtype")
                // instead of the serializedName of the polymorphicDiscriminator (ex: "fish.type")
                // is a better approach. The generator is not consistent with escaping '\.' in the
                // serializedName of the property (ex: "fish\.type") that is marked as polymorphic discriminator
                // and the serializedName of the metadata polymorphicDiscriminator (ex: "fish.type"). However,
                // the clientName transformation of the polymorphicDiscriminator (ex: "fishtype") and
                // the transformation of model property name (ex: "fishtype") is done consistently.
                // Hence, it is a safer bet to rely on the clientName of the polymorphicDiscriminator.
                if (polymorphicDiscriminator && key === polymorphicDiscriminator.clientName && propertyInstance == undefined) {
                    propertyInstance = mapper.serializedName;
                }
                var serializedValue = void 0;
                // paging
                if (Array.isArray(responseBody[key]) && modelProps[key].serializedName === "") {
                    propertyInstance = responseBody[key];
                    instance = serializer.deserialize(propertyMapper, propertyInstance, propertyObjectName);
                }
                else if (propertyInstance !== undefined || propertyMapper.defaultValue !== undefined) {
                    serializedValue = serializer.deserialize(propertyMapper, propertyInstance, propertyObjectName);
                    instance[key] = serializedValue;
                }
            }
        }
        var additionalPropertiesMapper = mapper.type.additionalProperties;
        if (additionalPropertiesMapper) {
            var isAdditionalProperty = function (responsePropName) {
                for (var clientPropName in modelProps) {
                    var paths = splitSerializeName(modelProps[clientPropName].serializedName);
                    if (paths[0] === responsePropName) {
                        return false;
                    }
                }
                return true;
            };
            for (var responsePropName in responseBody) {
                if (isAdditionalProperty(responsePropName)) {
                    instance[responsePropName] = serializer.deserialize(additionalPropertiesMapper, responseBody[responsePropName], objectName + '["' + responsePropName + '"]');
                }
            }
        }
        else if (responseBody) {
            for (var _e = 0, _f = Object.keys(responseBody); _e < _f.length; _e++) {
                var key = _f[_e];
                if (instance[key] === undefined && !handledPropertyNames.includes(key) && !isSpecialXmlProperty(key)) {
                    instance[key] = responseBody[key];
                }
            }
        }
        return instance;
    }
    function deserializeDictionaryType(serializer, mapper, responseBody, objectName) {
        /*jshint validthis: true */
        var value = mapper.type.value;
        if (!value || typeof value !== "object") {
            throw new Error("\"value\" metadata for a Dictionary must be defined in the " +
                ("mapper and it must of type \"object\" in " + objectName));
        }
        if (responseBody) {
            var tempDictionary = {};
            for (var _i = 0, _a = Object.keys(responseBody); _i < _a.length; _i++) {
                var key = _a[_i];
                tempDictionary[key] = serializer.deserialize(value, responseBody[key], objectName);
            }
            return tempDictionary;
        }
        return responseBody;
    }
    function deserializeSequenceType(serializer, mapper, responseBody, objectName) {
        /*jshint validthis: true */
        var element = mapper.type.element;
        if (!element || typeof element !== "object") {
            throw new Error("element\" metadata for an Array must be defined in the " +
                ("mapper and it must of type \"object\" in " + objectName));
        }
        if (responseBody) {
            if (!Array.isArray(responseBody)) {
                // xml2js will interpret a single element array as just the element, so force it to be an array
                responseBody = [responseBody];
            }
            var tempArray = [];
            for (var i = 0; i < responseBody.length; i++) {
                tempArray[i] = serializer.deserialize(element, responseBody[i], objectName + "[" + i + "]");
            }
            return tempArray;
        }
        return responseBody;
    }
    function getPolymorphicMapper(serializer, mapper, object, polymorphicPropertyName) {
        var polymorphicDiscriminator = getPolymorphicDiscriminatorRecursively(serializer, mapper);
        if (polymorphicDiscriminator) {
            var discriminatorName = polymorphicDiscriminator[polymorphicPropertyName];
            if (discriminatorName != undefined) {
                var discriminatorValue = object[discriminatorName];
                if (discriminatorValue != undefined) {
                    var typeName = mapper.type.uberParent || mapper.type.className;
                    var indexDiscriminator = discriminatorValue === typeName
                        ? discriminatorValue
                        : typeName + "." + discriminatorValue;
                    var polymorphicMapper = serializer.modelMappers.discriminators[indexDiscriminator];
                    if (polymorphicMapper) {
                        mapper = polymorphicMapper;
                    }
                }
            }
        }
        return mapper;
    }
    function getPolymorphicDiscriminatorRecursively(serializer, mapper) {
        return mapper.type.polymorphicDiscriminator
            || getPolymorphicDiscriminatorSafely(serializer, mapper.type.uberParent)
            || getPolymorphicDiscriminatorSafely(serializer, mapper.type.className);
    }
    function getPolymorphicDiscriminatorSafely(serializer, typeName) {
        return (typeName && serializer.modelMappers[typeName] && serializer.modelMappers[typeName].type.polymorphicDiscriminator);
    }
    /**
     * Utility function to create a K:V from a list of strings
     */
    function strEnum(o) {
        var result = {};
        for (var _i = 0, o_1 = o; _i < o_1.length; _i++) {
            var key = o_1[_i];
            result[key] = key;
        }
        return result;
    }
    var MapperType = strEnum([
        "Base64Url",
        "Boolean",
        "ByteArray",
        "Composite",
        "Date",
        "DateTime",
        "DateTimeRfc1123",
        "Dictionary",
        "Enum",
        "Number",
        "Object",
        "Sequence",
        "String",
        "Stream",
        "TimeSpan",
        "UnixTime"
    ]);

    // Copyright (c) Microsoft Corporation. All rights reserved.
    /**
     * Creates a new WebResource object.
     *
     * This class provides an abstraction over a REST call by being library / implementation agnostic and wrapping the necessary
     * properties to initiate a request.
     *
     * @constructor
     */
    var WebResource = /** @class */ (function () {
        function WebResource(url, method, body, query, headers, streamResponseBody, withCredentials, abortSignal, timeout, onUploadProgress, onDownloadProgress, proxySettings, keepAlive) {
            this.streamResponseBody = streamResponseBody;
            this.url = url || "";
            this.method = method || "GET";
            this.headers = (headers instanceof HttpHeaders ? headers : new HttpHeaders(headers));
            this.body = body;
            this.query = query;
            this.formData = undefined;
            this.withCredentials = withCredentials || false;
            this.abortSignal = abortSignal;
            this.timeout = timeout || 0;
            this.onUploadProgress = onUploadProgress;
            this.onDownloadProgress = onDownloadProgress;
            this.proxySettings = proxySettings;
            this.keepAlive = keepAlive;
        }
        /**
         * Validates that the required properties such as method, url, headers["Content-Type"],
         * headers["accept-language"] are defined. It will throw an error if one of the above
         * mentioned properties are not defined.
         */
        WebResource.prototype.validateRequestProperties = function () {
            if (!this.method) {
                throw new Error("WebResource.method is required.");
            }
            if (!this.url) {
                throw new Error("WebResource.url is required.");
            }
        };
        /**
         * Prepares the request.
         * @param {RequestPrepareOptions} options Options to provide for preparing the request.
         * @returns {WebResource} Returns the prepared WebResource (HTTP Request) object that needs to be given to the request pipeline.
         */
        WebResource.prototype.prepare = function (options) {
            if (!options) {
                throw new Error("options object is required");
            }
            if (options.method == undefined || typeof options.method.valueOf() !== "string") {
                throw new Error("options.method must be a string.");
            }
            if (options.url && options.pathTemplate) {
                throw new Error("options.url and options.pathTemplate are mutually exclusive. Please provide exactly one of them.");
            }
            if ((options.pathTemplate == undefined || typeof options.pathTemplate.valueOf() !== "string") && (options.url == undefined || typeof options.url.valueOf() !== "string")) {
                throw new Error("Please provide exactly one of options.pathTemplate or options.url.");
            }
            // set the url if it is provided.
            if (options.url) {
                if (typeof options.url !== "string") {
                    throw new Error("options.url must be of type \"string\".");
                }
                this.url = options.url;
            }
            // set the method
            if (options.method) {
                var validMethods = ["GET", "PUT", "HEAD", "DELETE", "OPTIONS", "POST", "PATCH", "TRACE"];
                if (validMethods.indexOf(options.method.toUpperCase()) === -1) {
                    throw new Error("The provided method \"" + options.method + "\" is invalid. Supported HTTP methods are: " + JSON.stringify(validMethods));
                }
            }
            this.method = options.method.toUpperCase();
            // construct the url if path template is provided
            if (options.pathTemplate) {
                var pathTemplate_1 = options.pathTemplate, pathParameters_1 = options.pathParameters;
                if (typeof pathTemplate_1 !== "string") {
                    throw new Error("options.pathTemplate must be of type \"string\".");
                }
                if (!options.baseUrl) {
                    options.baseUrl = "https://management.azure.com";
                }
                var baseUrl = options.baseUrl;
                var url_1 = baseUrl + (baseUrl.endsWith("/") ? "" : "/") + (pathTemplate_1.startsWith("/") ? pathTemplate_1.slice(1) : pathTemplate_1);
                var segments = url_1.match(/({\w*\s*\w*})/ig);
                if (segments && segments.length) {
                    if (!pathParameters_1) {
                        throw new Error("pathTemplate: " + pathTemplate_1 + " has been provided. Hence, options.pathParameters must also be provided.");
                    }
                    segments.forEach(function (item) {
                        var pathParamName = item.slice(1, -1);
                        var pathParam = pathParameters_1[pathParamName];
                        if (pathParam === null || pathParam === undefined || !(typeof pathParam === "string" || typeof pathParam === "object")) {
                            throw new Error("pathTemplate: " + pathTemplate_1 + " contains the path parameter " + pathParamName +
                                (" however, it is not present in " + pathParameters_1 + " - " + JSON.stringify(pathParameters_1, undefined, 2) + ".") +
                                ("The value of the path parameter can either be a \"string\" of the form { " + pathParamName + ": \"some sample value\" } or ") +
                                ("it can be an \"object\" of the form { \"" + pathParamName + "\": { value: \"some sample value\", skipUrlEncoding: true } }."));
                        }
                        if (typeof pathParam.valueOf() === "string") {
                            url_1 = url_1.replace(item, encodeURIComponent(pathParam));
                        }
                        if (typeof pathParam.valueOf() === "object") {
                            if (!pathParam.value) {
                                throw new Error("options.pathParameters[" + pathParamName + "] is of type \"object\" but it does not contain a \"value\" property.");
                            }
                            if (pathParam.skipUrlEncoding) {
                                url_1 = url_1.replace(item, pathParam.value);
                            }
                            else {
                                url_1 = url_1.replace(item, encodeURIComponent(pathParam.value));
                            }
                        }
                    });
                }
                this.url = url_1;
            }
            // append query parameters to the url if they are provided. They can be provided with pathTemplate or url option.
            if (options.queryParameters) {
                var queryParameters = options.queryParameters;
                if (typeof queryParameters !== "object") {
                    throw new Error("options.queryParameters must be of type object. It should be a JSON object " +
                        "of \"query-parameter-name\" as the key and the \"query-parameter-value\" as the value. " +
                        "The \"query-parameter-value\" may be fo type \"string\" or an \"object\" of the form { value: \"query-parameter-value\", skipUrlEncoding: true }.");
                }
                // append question mark if it is not present in the url
                if (this.url && this.url.indexOf("?") === -1) {
                    this.url += "?";
                }
                // construct queryString
                var queryParams = [];
                // We need to populate this.query as a dictionary if the request is being used for Sway's validateRequest().
                this.query = {};
                for (var queryParamName in queryParameters) {
                    var queryParam = queryParameters[queryParamName];
                    if (queryParam) {
                        if (typeof queryParam === "string") {
                            queryParams.push(queryParamName + "=" + encodeURIComponent(queryParam));
                            this.query[queryParamName] = encodeURIComponent(queryParam);
                        }
                        else if (typeof queryParam === "object") {
                            if (!queryParam.value) {
                                throw new Error("options.queryParameters[" + queryParamName + "] is of type \"object\" but it does not contain a \"value\" property.");
                            }
                            if (queryParam.skipUrlEncoding) {
                                queryParams.push(queryParamName + "=" + queryParam.value);
                                this.query[queryParamName] = queryParam.value;
                            }
                            else {
                                queryParams.push(queryParamName + "=" + encodeURIComponent(queryParam.value));
                                this.query[queryParamName] = encodeURIComponent(queryParam.value);
                            }
                        }
                    }
                } // end-of-for
                // append the queryString
                this.url += queryParams.join("&");
            }
            // add headers to the request if they are provided
            if (options.headers) {
                var headers = options.headers;
                for (var _i = 0, _a = Object.keys(options.headers); _i < _a.length; _i++) {
                    var headerName = _a[_i];
                    this.headers.set(headerName, headers[headerName]);
                }
            }
            // ensure accept-language is set correctly
            if (!this.headers.get("accept-language")) {
                this.headers.set("accept-language", "en-US");
            }
            // ensure the request-id is set correctly
            if (!this.headers.get("x-ms-client-request-id") && !options.disableClientRequestId) {
                this.headers.set("x-ms-client-request-id", generateUuid());
            }
            // default
            if (!this.headers.get("Content-Type")) {
                this.headers.set("Content-Type", "application/json; charset=utf-8");
            }
            // set the request body. request.js automatically sets the Content-Length request header, so we need not set it explicilty
            this.body = options.body;
            if (options.body != undefined) {
                // body as a stream special case. set the body as-is and check for some special request headers specific to sending a stream.
                if (options.bodyIsStream) {
                    if (!this.headers.get("Transfer-Encoding")) {
                        this.headers.set("Transfer-Encoding", "chunked");
                    }
                    if (this.headers.get("Content-Type") !== "application/octet-stream") {
                        this.headers.set("Content-Type", "application/octet-stream");
                    }
                }
                else {
                    if (options.serializationMapper) {
                        this.body = new Serializer(options.mappers).serialize(options.serializationMapper, options.body, "requestBody");
                    }
                    if (!options.disableJsonStringifyOnBody) {
                        this.body = JSON.stringify(options.body);
                    }
                }
            }
            this.abortSignal = options.abortSignal;
            this.onDownloadProgress = options.onDownloadProgress;
            this.onUploadProgress = options.onUploadProgress;
            return this;
        };
        /**
         * Clone this WebResource HTTP request object.
         * @returns {WebResource} The clone of this WebResource HTTP request object.
         */
        WebResource.prototype.clone = function () {
            var result = new WebResource(this.url, this.method, this.body, this.query, this.headers && this.headers.clone(), this.streamResponseBody, this.withCredentials, this.abortSignal, this.timeout, this.onUploadProgress, this.onDownloadProgress);
            if (this.formData) {
                result.formData = this.formData;
            }
            if (this.operationSpec) {
                result.operationSpec = this.operationSpec;
            }
            if (this.shouldDeserialize) {
                result.shouldDeserialize = this.shouldDeserialize;
            }
            if (this.operationResponseGetter) {
                result.operationResponseGetter = this.operationResponseGetter;
            }
            return result;
        };
        return WebResource;
    }());

    /*! *****************************************************************************
    Copyright (c) Microsoft Corporation. All rights reserved.
    Licensed under the Apache License, Version 2.0 (the "License"); you may not use
    this file except in compliance with the License. You may obtain a copy of the
    License at http://www.apache.org/licenses/LICENSE-2.0

    THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
    KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED
    WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
    MERCHANTABLITY OR NON-INFRINGEMENT.

    See the Apache Version 2.0 License for specific language governing permissions
    and limitations under the License.
    ***************************************************************************** */
    /* global Reflect, Promise */

    var extendStatics = function(d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };

    function __extends(d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    }

    var __assign = function() {
        __assign = Object.assign || function __assign(t) {
            for (var s, i = 1, n = arguments.length; i < n; i++) {
                s = arguments[i];
                for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p)) t[p] = s[p];
            }
            return t;
        };
        return __assign.apply(this, arguments);
    };

    function __awaiter(thisArg, _arguments, P, generator) {
        return new (P || (P = Promise))(function (resolve, reject) {
            function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
            function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
            function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
            step((generator = generator.apply(thisArg, _arguments || [])).next());
        });
    }

    function __generator(thisArg, body) {
        var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
        return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
        function verb(n) { return function (v) { return step([n, v]); }; }
        function step(op) {
            if (f) throw new TypeError("Generator is already executing.");
            while (_) try {
                if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
                if (y = 0, t) op = [op[0] & 2, t.value];
                switch (op[0]) {
                    case 0: case 1: t = op; break;
                    case 4: _.label++; return { value: op[1], done: false };
                    case 5: _.label++; y = op[1]; op = [0]; continue;
                    case 7: op = _.ops.pop(); _.trys.pop(); continue;
                    default:
                        if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                        if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                        if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                        if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                        if (t[2]) _.ops.pop();
                        _.trys.pop(); continue;
                }
                op = body.call(thisArg, _);
            } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
            if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
        }
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    var RestError = /** @class */ (function (_super) {
        __extends(RestError, _super);
        function RestError(message, code, statusCode, request, response, body) {
            var _this = _super.call(this, message) || this;
            _this.code = code;
            _this.statusCode = statusCode;
            _this.request = request;
            _this.response = response;
            _this.body = body;
            Object.setPrototypeOf(_this, RestError.prototype);
            return _this;
        }
        RestError.REQUEST_SEND_ERROR = "REQUEST_SEND_ERROR";
        RestError.REQUEST_ABORTED_ERROR = "REQUEST_ABORTED_ERROR";
        RestError.PARSE_ERROR = "PARSE_ERROR";
        return RestError;
    }(Error));

    // Copyright (c) Microsoft Corporation. All rights reserved.
    /**
     * A HttpClient implementation that uses XMLHttpRequest to send HTTP requests.
     */
    var XhrHttpClient = /** @class */ (function () {
        function XhrHttpClient() {
        }
        XhrHttpClient.prototype.sendRequest = function (request) {
            var xhr = new XMLHttpRequest();
            if (request.proxySettings) {
                throw new Error("HTTP proxy is not supported in browser environment");
            }
            var abortSignal = request.abortSignal;
            if (abortSignal) {
                var listener_1 = function () {
                    xhr.abort();
                };
                abortSignal.addEventListener("abort", listener_1);
                xhr.addEventListener("readystatechange", function () {
                    if (xhr.readyState === XMLHttpRequest.DONE) {
                        abortSignal.removeEventListener("abort", listener_1);
                    }
                });
            }
            addProgressListener(xhr.upload, request.onUploadProgress);
            addProgressListener(xhr, request.onDownloadProgress);
            if (request.formData) {
                var formData = request.formData;
                var requestForm_1 = new FormData();
                var appendFormValue = function (key, value) {
                    if (value && value.hasOwnProperty("value") && value.hasOwnProperty("options")) {
                        requestForm_1.append(key, value.value, value.options);
                    }
                    else {
                        requestForm_1.append(key, value);
                    }
                };
                for (var _i = 0, _a = Object.keys(formData); _i < _a.length; _i++) {
                    var formKey = _a[_i];
                    var formValue = formData[formKey];
                    if (Array.isArray(formValue)) {
                        for (var j = 0; j < formValue.length; j++) {
                            appendFormValue(formKey, formValue[j]);
                        }
                    }
                    else {
                        appendFormValue(formKey, formValue);
                    }
                }
                request.body = requestForm_1;
                request.formData = undefined;
                var contentType = request.headers.get("Content-Type");
                if (contentType && contentType.indexOf("multipart/form-data") !== -1) {
                    // browser will automatically apply a suitable content-type header
                    request.headers.remove("Content-Type");
                }
            }
            xhr.open(request.method, request.url);
            xhr.timeout = request.timeout;
            xhr.withCredentials = request.withCredentials;
            for (var _b = 0, _c = request.headers.headersArray(); _b < _c.length; _b++) {
                var header = _c[_b];
                xhr.setRequestHeader(header.name, header.value);
            }
            xhr.responseType = request.streamResponseBody ? "blob" : "text";
            // tslint:disable-next-line:no-null-keyword
            xhr.send(request.body === undefined ? null : request.body);
            if (request.streamResponseBody) {
                return new Promise(function (resolve, reject) {
                    xhr.addEventListener("readystatechange", function () {
                        // Resolve as soon as headers are loaded
                        if (xhr.readyState === XMLHttpRequest.HEADERS_RECEIVED) {
                            var blobBody = new Promise(function (resolve, reject) {
                                xhr.addEventListener("load", function () {
                                    resolve(xhr.response);
                                });
                                rejectOnTerminalEvent(request, xhr, reject);
                            });
                            resolve({
                                request: request,
                                status: xhr.status,
                                headers: parseHeaders(xhr),
                                blobBody: blobBody
                            });
                        }
                    });
                    rejectOnTerminalEvent(request, xhr, reject);
                });
            }
            else {
                return new Promise(function (resolve, reject) {
                    xhr.addEventListener("load", function () { return resolve({
                        request: request,
                        status: xhr.status,
                        headers: parseHeaders(xhr),
                        bodyAsText: xhr.responseText
                    }); });
                    rejectOnTerminalEvent(request, xhr, reject);
                });
            }
        };
        return XhrHttpClient;
    }());
    function addProgressListener(xhr, listener) {
        if (listener) {
            xhr.addEventListener("progress", function (rawEvent) { return listener({
                loadedBytes: rawEvent.loaded
            }); });
        }
    }
    // exported locally for testing
    function parseHeaders(xhr) {
        var responseHeaders = new HttpHeaders();
        var headerLines = xhr.getAllResponseHeaders().trim().split(/[\r\n]+/);
        for (var _i = 0, headerLines_1 = headerLines; _i < headerLines_1.length; _i++) {
            var line = headerLines_1[_i];
            var index = line.indexOf(":");
            var headerName = line.slice(0, index);
            var headerValue = line.slice(index + 2);
            responseHeaders.set(headerName, headerValue);
        }
        return responseHeaders;
    }
    function rejectOnTerminalEvent(request, xhr, reject) {
        xhr.addEventListener("error", function () { return reject(new RestError("Failed to send request to " + request.url, RestError.REQUEST_SEND_ERROR, undefined, request)); });
        xhr.addEventListener("abort", function () { return reject(new RestError("The request was aborted", RestError.REQUEST_ABORTED_ERROR, undefined, request)); });
        xhr.addEventListener("timeout", function () { return reject(new RestError("timeout of " + xhr.timeout + "ms exceeded", RestError.REQUEST_SEND_ERROR, undefined, request)); });
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    (function (HttpPipelineLogLevel) {
        /**
         * A log level that indicates that no logs will be logged.
         */
        HttpPipelineLogLevel[HttpPipelineLogLevel["OFF"] = 0] = "OFF";
        /**
         * An error log.
         */
        HttpPipelineLogLevel[HttpPipelineLogLevel["ERROR"] = 1] = "ERROR";
        /**
         * A warning log.
         */
        HttpPipelineLogLevel[HttpPipelineLogLevel["WARNING"] = 2] = "WARNING";
        /**
         * An information log.
         */
        HttpPipelineLogLevel[HttpPipelineLogLevel["INFO"] = 3] = "INFO";
    })(exports.HttpPipelineLogLevel || (exports.HttpPipelineLogLevel = {}));

    // Copyright (c) Microsoft Corporation. All rights reserved.
    // Licensed under the MIT License. See License.txt in the project root for license information.
    /**
     * Get the path to this parameter's value as a dotted string (a.b.c).
     * @param parameter The parameter to get the path string for.
     * @returns The path to this parameter's value as a dotted string.
     */
    function getPathStringFromParameter(parameter) {
        return getPathStringFromParameterPath(parameter.parameterPath, parameter.mapper);
    }
    function getPathStringFromParameterPath(parameterPath, mapper) {
        var result;
        if (typeof parameterPath === "string") {
            result = parameterPath;
        }
        else if (Array.isArray(parameterPath)) {
            result = parameterPath.join(".");
        }
        else {
            result = mapper.serializedName;
        }
        return result;
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    function isStreamOperation(operationSpec) {
        var result = false;
        for (var statusCode in operationSpec.responses) {
            var operationResponse = operationSpec.responses[statusCode];
            if (operationResponse.bodyMapper && operationResponse.bodyMapper.type.name === MapperType.Stream) {
                result = true;
                break;
            }
        }
        return result;
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    // Licensed under the MIT License. See License.txt in the project root for license information.
    var parser = new DOMParser();
    function parseXML(str) {
        try {
            var dom = parser.parseFromString(str, "application/xml");
            throwIfError(dom);
            var obj = domToObject(dom.childNodes[0]);
            return Promise.resolve(obj);
        }
        catch (err) {
            return Promise.reject(err);
        }
    }
    var errorNS = "";
    try {
        errorNS = parser.parseFromString("INVALID", "text/xml").getElementsByTagName("parsererror")[0].namespaceURI;
    }
    catch (ignored) {
        // Most browsers will return a document containing <parsererror>, but IE will throw.
    }
    function throwIfError(dom) {
        if (errorNS) {
            var parserErrors = dom.getElementsByTagNameNS(errorNS, "parsererror");
            if (parserErrors.length) {
                throw new Error(parserErrors.item(0).innerHTML);
            }
        }
    }
    function isElement(node) {
        return !!node.attributes;
    }
    /**
     * Get the Element-typed version of the provided Node if the provided node is an element with
     * attributes. If it isn't, then undefined is returned.
     */
    function asElementWithAttributes(node) {
        return isElement(node) && node.hasAttributes() ? node : undefined;
    }
    function domToObject(node) {
        var result = {};
        var childNodeCount = node.childNodes.length;
        var firstChildNode = node.childNodes[0];
        var onlyChildTextValue = (firstChildNode && childNodeCount === 1 && firstChildNode.nodeType === Node.TEXT_NODE && firstChildNode.nodeValue) || undefined;
        var elementWithAttributes = asElementWithAttributes(node);
        if (elementWithAttributes) {
            result["$"] = {};
            for (var i = 0; i < elementWithAttributes.attributes.length; i++) {
                var attr = elementWithAttributes.attributes[i];
                result["$"][attr.nodeName] = attr.nodeValue;
            }
            if (onlyChildTextValue) {
                result["_"] = onlyChildTextValue;
            }
        }
        else if (childNodeCount === 0) {
            result = "";
        }
        else if (onlyChildTextValue) {
            result = onlyChildTextValue;
        }
        if (!onlyChildTextValue) {
            for (var i = 0; i < childNodeCount; i++) {
                var child = node.childNodes[i];
                // Ignore leading/trailing whitespace nodes
                if (child.nodeType !== Node.TEXT_NODE) {
                    var childObject = domToObject(child);
                    if (!result[child.nodeName]) {
                        result[child.nodeName] = childObject;
                    }
                    else if (Array.isArray(result[child.nodeName])) {
                        result[child.nodeName].push(childObject);
                    }
                    else {
                        result[child.nodeName] = [result[child.nodeName], childObject];
                    }
                }
            }
        }
        return result;
    }
    // tslint:disable-next-line:no-null-keyword
    var doc = document.implementation.createDocument(null, null, null);
    var serializer = new XMLSerializer();
    function stringifyXML(obj, opts) {
        var rootName = opts && opts.rootName || "root";
        var dom = buildNode(obj, rootName)[0];
        return '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>' + serializer.serializeToString(dom);
    }
    function buildAttributes(attrs) {
        var result = [];
        for (var _i = 0, _a = Object.keys(attrs); _i < _a.length; _i++) {
            var key = _a[_i];
            var attr = doc.createAttribute(key);
            attr.value = attrs[key].toString();
            result.push(attr);
        }
        return result;
    }
    function buildNode(obj, elementName) {
        if (typeof obj === "string" || typeof obj === "number" || typeof obj === "boolean") {
            var elem = doc.createElement(elementName);
            elem.textContent = obj.toString();
            return [elem];
        }
        else if (Array.isArray(obj)) {
            var result = [];
            for (var _i = 0, obj_1 = obj; _i < obj_1.length; _i++) {
                var arrayElem = obj_1[_i];
                for (var _a = 0, _b = buildNode(arrayElem, elementName); _a < _b.length; _a++) {
                    var child = _b[_a];
                    result.push(child);
                }
            }
            return result;
        }
        else if (typeof obj === "object") {
            var elem = doc.createElement(elementName);
            for (var _c = 0, _d = Object.keys(obj); _c < _d.length; _c++) {
                var key = _d[_c];
                if (key === "$") {
                    for (var _e = 0, _f = buildAttributes(obj[key]); _e < _f.length; _e++) {
                        var attr = _f[_e];
                        elem.attributes.setNamedItem(attr);
                    }
                }
                else {
                    for (var _g = 0, _h = buildNode(obj[key], key); _g < _h.length; _g++) {
                        var child = _h[_g];
                        elem.appendChild(child);
                    }
                }
            }
            return [elem];
        }
        else {
            throw new Error("Illegal value passed to buildObject: " + obj);
        }
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    var BaseRequestPolicy = /** @class */ (function () {
        function BaseRequestPolicy(_nextPolicy, _options) {
            this._nextPolicy = _nextPolicy;
            this._options = _options;
        }
        /**
         * Get whether or not a log with the provided log level should be logged.
         * @param logLevel The log level of the log that will be logged.
         * @returns Whether or not a log with the provided log level should be logged.
         */
        BaseRequestPolicy.prototype.shouldLog = function (logLevel) {
            return this._options.shouldLog(logLevel);
        };
        /**
         * Attempt to log the provided message to the provided logger. If no logger was provided or if
         * the log level does not meat the logger's threshold, then nothing will be logged.
         * @param logLevel The log level of this log.
         * @param message The message of this log.
         */
        BaseRequestPolicy.prototype.log = function (logLevel, message) {
            this._options.log(logLevel, message);
        };
        return BaseRequestPolicy;
    }());
    /**
     * Optional properties that can be used when creating a RequestPolicy.
     */
    var RequestPolicyOptions = /** @class */ (function () {
        function RequestPolicyOptions(_logger) {
            this._logger = _logger;
        }
        /**
         * Get whether or not a log with the provided log level should be logged.
         * @param logLevel The log level of the log that will be logged.
         * @returns Whether or not a log with the provided log level should be logged.
         */
        RequestPolicyOptions.prototype.shouldLog = function (logLevel) {
            return !!this._logger &&
                logLevel !== exports.HttpPipelineLogLevel.OFF &&
                logLevel <= this._logger.minimumLogLevel;
        };
        /**
         * Attempt to log the provided message to the provided logger. If no logger was provided or if
         * the log level does not meat the logger's threshold, then nothing will be logged.
         * @param logLevel The log level of this log.
         * @param message The message of this log.
         */
        RequestPolicyOptions.prototype.log = function (logLevel, message) {
            if (this._logger && this.shouldLog(logLevel)) {
                this._logger.log(logLevel, message);
            }
        };
        return RequestPolicyOptions;
    }());

    // Copyright (c) Microsoft Corporation. All rights reserved.
    /**
     * Create a new serialization RequestPolicyCreator that will serialized HTTP request bodies as they
     * pass through the HTTP pipeline.
     */
    function deserializationPolicy(deserializationContentTypes) {
        return {
            create: function (nextPolicy, options) {
                return new DeserializationPolicy(nextPolicy, deserializationContentTypes, options);
            }
        };
    }
    var defaultJsonContentTypes = ["application/json", "text/json"];
    var defaultXmlContentTypes = ["application/xml", "application/atom+xml"];
    /**
     * A RequestPolicy that will deserialize HTTP response bodies and headers as they pass through the
     * HTTP pipeline.
     */
    var DeserializationPolicy = /** @class */ (function (_super) {
        __extends(DeserializationPolicy, _super);
        function DeserializationPolicy(nextPolicy, deserializationContentTypes, options) {
            var _this = _super.call(this, nextPolicy, options) || this;
            _this.jsonContentTypes = deserializationContentTypes && deserializationContentTypes.json || defaultJsonContentTypes;
            _this.xmlContentTypes = deserializationContentTypes && deserializationContentTypes.xml || defaultXmlContentTypes;
            return _this;
        }
        DeserializationPolicy.prototype.sendRequest = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                var _this = this;
                return __generator(this, function (_a) {
                    return [2 /*return*/, this._nextPolicy.sendRequest(request).then(function (response) { return deserializeResponseBody(_this.jsonContentTypes, _this.xmlContentTypes, response); })];
                });
            });
        };
        return DeserializationPolicy;
    }(BaseRequestPolicy));
    function getOperationResponse(parsedResponse) {
        var result;
        var request = parsedResponse.request;
        var operationSpec = request.operationSpec;
        if (operationSpec) {
            var operationResponseGetter = request.operationResponseGetter;
            if (!operationResponseGetter) {
                result = operationSpec.responses[parsedResponse.status];
            }
            else {
                result = operationResponseGetter(operationSpec, parsedResponse);
            }
        }
        return result;
    }
    function shouldDeserializeResponse(parsedResponse) {
        var shouldDeserialize = parsedResponse.request.shouldDeserialize;
        var result;
        if (shouldDeserialize === undefined) {
            result = true;
        }
        else if (typeof shouldDeserialize === "boolean") {
            result = shouldDeserialize;
        }
        else {
            result = shouldDeserialize(parsedResponse);
        }
        return result;
    }
    function deserializeResponseBody(jsonContentTypes, xmlContentTypes, response) {
        return parse(jsonContentTypes, xmlContentTypes, response).then(function (parsedResponse) {
            var shouldDeserialize = shouldDeserializeResponse(parsedResponse);
            if (shouldDeserialize) {
                var operationSpec = parsedResponse.request.operationSpec;
                if (operationSpec && operationSpec.responses) {
                    var statusCode = parsedResponse.status;
                    var expectedStatusCodes = Object.keys(operationSpec.responses);
                    var hasNoExpectedStatusCodes = (expectedStatusCodes.length === 0 || (expectedStatusCodes.length === 1 && expectedStatusCodes[0] === "default"));
                    var responseSpec = getOperationResponse(parsedResponse);
                    var isExpectedStatusCode = hasNoExpectedStatusCodes ? (200 <= statusCode && statusCode < 300) : !!responseSpec;
                    if (!isExpectedStatusCode) {
                        var defaultResponseSpec = operationSpec.responses.default;
                        if (defaultResponseSpec) {
                            var initialErrorMessage = isStreamOperation(operationSpec)
                                ? "Unexpected status code: " + statusCode
                                : parsedResponse.bodyAsText;
                            var error = new RestError(initialErrorMessage);
                            error.statusCode = statusCode;
                            error.request = stripRequest(parsedResponse.request);
                            error.response = stripResponse(parsedResponse);
                            var parsedErrorResponse = parsedResponse.parsedBody;
                            try {
                                if (parsedErrorResponse) {
                                    var defaultResponseBodyMapper = defaultResponseSpec.bodyMapper;
                                    if (defaultResponseBodyMapper && defaultResponseBodyMapper.serializedName === "CloudError") {
                                        if (parsedErrorResponse.error) {
                                            parsedErrorResponse = parsedErrorResponse.error;
                                        }
                                        if (parsedErrorResponse.code) {
                                            error.code = parsedErrorResponse.code;
                                        }
                                        if (parsedErrorResponse.message) {
                                            error.message = parsedErrorResponse.message;
                                        }
                                    }
                                    else {
                                        var internalError = parsedErrorResponse;
                                        if (parsedErrorResponse.error) {
                                            internalError = parsedErrorResponse.error;
                                        }
                                        error.code = internalError.code;
                                        if (internalError.message) {
                                            error.message = internalError.message;
                                        }
                                    }
                                    if (defaultResponseBodyMapper) {
                                        var valueToDeserialize = parsedErrorResponse;
                                        if (operationSpec.isXML && defaultResponseBodyMapper.type.name === MapperType.Sequence) {
                                            valueToDeserialize = typeof parsedErrorResponse === "object"
                                                ? parsedErrorResponse[defaultResponseBodyMapper.xmlElementName]
                                                : [];
                                        }
                                        error.body = operationSpec.serializer.deserialize(defaultResponseBodyMapper, valueToDeserialize, "error.body");
                                    }
                                }
                            }
                            catch (defaultError) {
                                error.message = "Error \"" + defaultError.message + "\" occurred in deserializing the responseBody - \"" + parsedResponse.bodyAsText + "\" for the default response.";
                            }
                            return Promise.reject(error);
                        }
                    }
                    else if (responseSpec) {
                        if (responseSpec.bodyMapper) {
                            var valueToDeserialize = parsedResponse.parsedBody;
                            if (operationSpec.isXML && responseSpec.bodyMapper.type.name === MapperType.Sequence) {
                                valueToDeserialize = typeof valueToDeserialize === "object" ? valueToDeserialize[responseSpec.bodyMapper.xmlElementName] : [];
                            }
                            try {
                                parsedResponse.parsedBody = operationSpec.serializer.deserialize(responseSpec.bodyMapper, valueToDeserialize, "operationRes.parsedBody");
                            }
                            catch (error) {
                                var restError = new RestError("Error " + error + " occurred in deserializing the responseBody - " + parsedResponse.bodyAsText);
                                restError.request = stripRequest(parsedResponse.request);
                                restError.response = stripResponse(parsedResponse);
                                return Promise.reject(restError);
                            }
                        }
                        else if (operationSpec.httpMethod === "HEAD") {
                            // head methods never have a body, but we return a boolean to indicate presence/absence of the resource
                            parsedResponse.parsedBody = response.status >= 200 && response.status < 300;
                        }
                        if (responseSpec.headersMapper) {
                            parsedResponse.parsedHeaders = operationSpec.serializer.deserialize(responseSpec.headersMapper, parsedResponse.headers.rawHeaders(), "operationRes.parsedHeaders");
                        }
                    }
                }
            }
            return Promise.resolve(parsedResponse);
        });
    }
    function parse(jsonContentTypes, xmlContentTypes, operationResponse) {
        var errorHandler = function (err) {
            var msg = "Error \"" + err + "\" occurred while parsing the response body - " + operationResponse.bodyAsText + ".";
            var errCode = err.code || RestError.PARSE_ERROR;
            var e = new RestError(msg, errCode, operationResponse.status, operationResponse.request, operationResponse, operationResponse.bodyAsText);
            return Promise.reject(e);
        };
        if (!operationResponse.request.streamResponseBody && operationResponse.bodyAsText) {
            var text_1 = operationResponse.bodyAsText;
            var contentType = operationResponse.headers.get("Content-Type") || "";
            var contentComponents = !contentType ? [] : contentType.split(";").map(function (component) { return component.toLowerCase(); });
            if (contentComponents.length === 0 || contentComponents.some(function (component) { return jsonContentTypes.indexOf(component) !== -1; })) {
                return new Promise(function (resolve) {
                    operationResponse.parsedBody = JSON.parse(text_1);
                    resolve(operationResponse);
                }).catch(errorHandler);
            }
            else if (contentComponents.some(function (component) { return xmlContentTypes.indexOf(component) !== -1; })) {
                return parseXML(text_1)
                    .then(function (body) {
                    operationResponse.parsedBody = body;
                    return operationResponse;
                })
                    .catch(errorHandler);
            }
        }
        return Promise.resolve(operationResponse);
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    function exponentialRetryPolicy(retryCount, retryInterval, minRetryInterval, maxRetryInterval) {
        return {
            create: function (nextPolicy, options) {
                return new ExponentialRetryPolicy(nextPolicy, options, retryCount, retryInterval, minRetryInterval, maxRetryInterval);
            }
        };
    }
    var DEFAULT_CLIENT_RETRY_INTERVAL = 1000 * 30;
    var DEFAULT_CLIENT_RETRY_COUNT = 3;
    var DEFAULT_CLIENT_MAX_RETRY_INTERVAL = 1000 * 90;
    var DEFAULT_CLIENT_MIN_RETRY_INTERVAL = 1000 * 3;
    /**
     * @class
     * Instantiates a new "ExponentialRetryPolicyFilter" instance.
     */
    var ExponentialRetryPolicy = /** @class */ (function (_super) {
        __extends(ExponentialRetryPolicy, _super);
        /**
         * @constructor
         * @param {RequestPolicy} nextPolicy The next RequestPolicy in the pipeline chain.
         * @param {RequestPolicyOptions} options The options for this RequestPolicy.
         * @param {number} [retryCount]        The client retry count.
         * @param {number} [retryInterval]     The client retry interval, in milliseconds.
         * @param {number} [minRetryInterval]  The minimum retry interval, in milliseconds.
         * @param {number} [maxRetryInterval]  The maximum retry interval, in milliseconds.
         */
        function ExponentialRetryPolicy(nextPolicy, options, retryCount, retryInterval, minRetryInterval, maxRetryInterval) {
            var _this = _super.call(this, nextPolicy, options) || this;
            function isNumber(n) { return typeof n === "number"; }
            _this.retryCount = isNumber(retryCount) ? retryCount : DEFAULT_CLIENT_RETRY_COUNT;
            _this.retryInterval = isNumber(retryInterval) ? retryInterval : DEFAULT_CLIENT_RETRY_INTERVAL;
            _this.minRetryInterval = isNumber(minRetryInterval) ? minRetryInterval : DEFAULT_CLIENT_MIN_RETRY_INTERVAL;
            _this.maxRetryInterval = isNumber(maxRetryInterval) ? maxRetryInterval : DEFAULT_CLIENT_MAX_RETRY_INTERVAL;
            return _this;
        }
        ExponentialRetryPolicy.prototype.sendRequest = function (request) {
            var _this = this;
            return this._nextPolicy.sendRequest(request.clone())
                .then(function (response) { return retry(_this, request, response); })
                .catch(function (error) { return retry(_this, request, error.response, undefined, error); });
        };
        return ExponentialRetryPolicy;
    }(BaseRequestPolicy));
    /**
     * Determines if the operation should be retried and how long to wait until the next retry.
     *
     * @param {ExponentialRetryPolicy} policy The ExponentialRetryPolicy that this function is being called against.
     * @param {number} statusCode The HTTP status code.
     * @param {RetryData} retryData  The retry data.
     * @return {boolean} True if the operation qualifies for a retry; false otherwise.
     */
    function shouldRetry(policy, statusCode, retryData) {
        if (statusCode == undefined || (statusCode < 500 && statusCode !== 408) || statusCode === 501 || statusCode === 505) {
            return false;
        }
        var currentCount;
        if (!retryData) {
            throw new Error("retryData for the ExponentialRetryPolicyFilter cannot be null.");
        }
        else {
            currentCount = (retryData && retryData.retryCount);
        }
        return (currentCount < policy.retryCount);
    }
    /**
     * Updates the retry data for the next attempt.
     *
     * @param {ExponentialRetryPolicy} policy The ExponentialRetryPolicy that this function is being called against.
     * @param {RetryData} retryData  The retry data.
     * @param {RetryError} [err] The operation"s error, if any.
     */
    function updateRetryData(policy, retryData, err) {
        if (!retryData) {
            retryData = {
                retryCount: 0,
                retryInterval: 0
            };
        }
        if (err) {
            if (retryData.error) {
                err.innerError = retryData.error;
            }
            retryData.error = err;
        }
        // Adjust retry count
        retryData.retryCount++;
        // Adjust retry interval
        var incrementDelta = Math.pow(2, retryData.retryCount) - 1;
        var boundedRandDelta = policy.retryInterval * 0.8 +
            Math.floor(Math.random() * (policy.retryInterval * 1.2 - policy.retryInterval * 0.8));
        incrementDelta *= boundedRandDelta;
        retryData.retryInterval = Math.min(policy.minRetryInterval + incrementDelta, policy.maxRetryInterval);
        return retryData;
    }
    function retry(policy, request, response, retryData, requestError) {
        retryData = updateRetryData(policy, retryData, requestError);
        var isAborted = request.abortSignal && request.abortSignal.aborted;
        if (!isAborted && shouldRetry(policy, response && response.status, retryData)) {
            return delay(retryData.retryInterval)
                .then(function () { return policy._nextPolicy.sendRequest(request.clone()); })
                .then(function (res) { return retry(policy, request, res, retryData, undefined); })
                .catch(function (err) { return retry(policy, request, response, retryData, err); });
        }
        else if (isAborted || requestError || !response) {
            // If the operation failed in the end, return all errors instead of just the last one
            var err = retryData.error ||
                new RestError("Failed to send the request.", RestError.REQUEST_SEND_ERROR, response && response.status, response && response.request, response);
            return Promise.reject(err);
        }
        else {
            return Promise.resolve(response);
        }
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    function generateClientRequestIdPolicy(requestIdHeaderName) {
        if (requestIdHeaderName === void 0) { requestIdHeaderName = "x-ms-client-request-id"; }
        return {
            create: function (nextPolicy, options) {
                return new GenerateClientRequestIdPolicy(nextPolicy, options, requestIdHeaderName);
            }
        };
    }
    var GenerateClientRequestIdPolicy = /** @class */ (function (_super) {
        __extends(GenerateClientRequestIdPolicy, _super);
        function GenerateClientRequestIdPolicy(nextPolicy, options, _requestIdHeaderName) {
            var _this = _super.call(this, nextPolicy, options) || this;
            _this._requestIdHeaderName = _requestIdHeaderName;
            return _this;
        }
        GenerateClientRequestIdPolicy.prototype.sendRequest = function (request) {
            if (!request.headers.contains(this._requestIdHeaderName)) {
                request.headers.set(this._requestIdHeaderName, generateUuid());
            }
            return this._nextPolicy.sendRequest(request);
        };
        return GenerateClientRequestIdPolicy;
    }(BaseRequestPolicy));

    // Copyright (c) Microsoft Corporation. All rights reserved.
    // Licensed under the MIT License. See License.txt in the project root for license information.
    function getDefaultUserAgentKey() {
        return "x-ms-command-name";
    }
    function getPlatformSpecificData() {
        var navigator = window.navigator;
        var osInfo = {
            key: "OS",
            value: (navigator.oscpu || navigator.platform).replace(" ", "")
        };
        return [osInfo];
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    function getRuntimeInfo() {
        var msRestRuntime = {
            key: "ms-rest-js",
            value: Constants.msRestVersion
        };
        return [msRestRuntime];
    }
    function getUserAgentString(telemetryInfo, keySeparator, valueSeparator) {
        if (keySeparator === void 0) { keySeparator = " "; }
        if (valueSeparator === void 0) { valueSeparator = "/"; }
        return telemetryInfo.map(function (info) {
            var value = info.value ? "" + valueSeparator + info.value : "";
            return "" + info.key + value;
        }).join(keySeparator);
    }
    var getDefaultUserAgentHeaderName = getDefaultUserAgentKey;
    function getDefaultUserAgentValue() {
        var runtimeInfo = getRuntimeInfo();
        var platformSpecificData = getPlatformSpecificData();
        var userAgent = getUserAgentString(runtimeInfo.concat(platformSpecificData));
        return userAgent;
    }
    function userAgentPolicy(userAgentData) {
        var key = (!userAgentData || userAgentData.key == undefined) ? getDefaultUserAgentKey() : userAgentData.key;
        var value = (!userAgentData || userAgentData.value == undefined) ? getDefaultUserAgentValue() : userAgentData.value;
        return {
            create: function (nextPolicy, options) {
                return new UserAgentPolicy(nextPolicy, options, key, value);
            }
        };
    }
    var UserAgentPolicy = /** @class */ (function (_super) {
        __extends(UserAgentPolicy, _super);
        function UserAgentPolicy(_nextPolicy, _options, headerKey, headerValue) {
            var _this = _super.call(this, _nextPolicy, _options) || this;
            _this._nextPolicy = _nextPolicy;
            _this._options = _options;
            _this.headerKey = headerKey;
            _this.headerValue = headerValue;
            return _this;
        }
        UserAgentPolicy.prototype.sendRequest = function (request) {
            this.addUserAgentHeader(request);
            return this._nextPolicy.sendRequest(request);
        };
        UserAgentPolicy.prototype.addUserAgentHeader = function (request) {
            if (!request.headers) {
                request.headers = new HttpHeaders();
            }
            if (!request.headers.get(this.headerKey) && this.headerValue) {
                request.headers.set(this.headerKey, this.headerValue);
            }
        };
        return UserAgentPolicy;
    }(BaseRequestPolicy));

    // Copyright (c) Microsoft Corporation. All rights reserved.
    /**
     * A class that handles the query portion of a URLBuilder.
     */
    var URLQuery = /** @class */ (function () {
        function URLQuery() {
            this._rawQuery = {};
        }
        /**
         * Get whether or not there any query parameters in this URLQuery.
         */
        URLQuery.prototype.any = function () {
            return Object.keys(this._rawQuery).length > 0;
        };
        /**
         * Set a query parameter with the provided name and value. If the parameterValue is undefined or
         * empty, then this will attempt to remove an existing query parameter with the provided
         * parameterName.
         */
        URLQuery.prototype.set = function (parameterName, parameterValue) {
            if (parameterName) {
                if (parameterValue != undefined) {
                    var newValue = Array.isArray(parameterValue) ? parameterValue : parameterValue.toString();
                    this._rawQuery[parameterName] = newValue;
                }
                else {
                    delete this._rawQuery[parameterName];
                }
            }
        };
        /**
         * Get the value of the query parameter with the provided name. If no parameter exists with the
         * provided parameter name, then undefined will be returned.
         */
        URLQuery.prototype.get = function (parameterName) {
            return parameterName ? this._rawQuery[parameterName] : undefined;
        };
        /**
         * Get the string representation of this query. The return value will not start with a "?".
         */
        URLQuery.prototype.toString = function () {
            var result = "";
            for (var parameterName in this._rawQuery) {
                if (result) {
                    result += "&";
                }
                var parameterValue = this._rawQuery[parameterName];
                if (Array.isArray(parameterValue)) {
                    var parameterStrings = [];
                    for (var _i = 0, parameterValue_1 = parameterValue; _i < parameterValue_1.length; _i++) {
                        var parameterValueElement = parameterValue_1[_i];
                        parameterStrings.push(parameterName + "=" + parameterValueElement);
                    }
                    result += parameterStrings.join("&");
                }
                else {
                    result += parameterName + "=" + parameterValue;
                }
            }
            return result;
        };
        /**
         * Parse a URLQuery from the provided text.
         */
        URLQuery.parse = function (text) {
            var result = new URLQuery();
            if (text) {
                if (text.startsWith("?")) {
                    text = text.substring(1);
                }
                var currentState = "ParameterName";
                var parameterName = "";
                var parameterValue = "";
                for (var i = 0; i < text.length; ++i) {
                    var currentCharacter = text[i];
                    switch (currentState) {
                        case "ParameterName":
                            switch (currentCharacter) {
                                case "=":
                                    currentState = "ParameterValue";
                                    break;
                                case "&":
                                    parameterName = "";
                                    parameterValue = "";
                                    break;
                                default:
                                    parameterName += currentCharacter;
                                    break;
                            }
                            break;
                        case "ParameterValue":
                            switch (currentCharacter) {
                                case "=":
                                    parameterName = "";
                                    parameterValue = "";
                                    currentState = "Invalid";
                                    break;
                                case "&":
                                    result.set(parameterName, parameterValue);
                                    parameterName = "";
                                    parameterValue = "";
                                    currentState = "ParameterName";
                                    break;
                                default:
                                    parameterValue += currentCharacter;
                                    break;
                            }
                            break;
                        case "Invalid":
                            if (currentCharacter === "&") {
                                currentState = "ParameterName";
                            }
                            break;
                        default:
                            throw new Error("Unrecognized URLQuery parse state: " + currentState);
                    }
                }
                if (currentState === "ParameterValue") {
                    result.set(parameterName, parameterValue);
                }
            }
            return result;
        };
        return URLQuery;
    }());
    /**
     * A class that handles creating, modifying, and parsing URLs.
     */
    var URLBuilder = /** @class */ (function () {
        function URLBuilder() {
        }
        /**
         * Set the scheme/protocol for this URL. If the provided scheme contains other parts of a URL
         * (such as a host, port, path, or query), those parts will be added to this URL as well.
         */
        URLBuilder.prototype.setScheme = function (scheme) {
            if (!scheme) {
                this._scheme = undefined;
            }
            else {
                this.set(scheme, "SCHEME");
            }
        };
        /**
         * Get the scheme that has been set in this URL.
         */
        URLBuilder.prototype.getScheme = function () {
            return this._scheme;
        };
        /**
         * Set the host for this URL. If the provided host contains other parts of a URL (such as a
         * port, path, or query), those parts will be added to this URL as well.
         */
        URLBuilder.prototype.setHost = function (host) {
            if (!host) {
                this._host = undefined;
            }
            else {
                this.set(host, "SCHEME_OR_HOST");
            }
        };
        /**
         * Get the host that has been set in this URL.
         */
        URLBuilder.prototype.getHost = function () {
            return this._host;
        };
        /**
         * Set the port for this URL. If the provided port contains other parts of a URL (such as a
         * path or query), those parts will be added to this URL as well.
         */
        URLBuilder.prototype.setPort = function (port) {
            if (port == undefined || port === "") {
                this._port = undefined;
            }
            else {
                this.set(port.toString(), "PORT");
            }
        };
        /**
         * Get the port that has been set in this URL.
         */
        URLBuilder.prototype.getPort = function () {
            return this._port;
        };
        /**
         * Set the path for this URL. If the provided path contains a query, then it will be added to
         * this URL as well.
         */
        URLBuilder.prototype.setPath = function (path) {
            if (!path) {
                this._path = undefined;
            }
            else {
                if (path.indexOf("://") !== -1) {
                    this.set(path, "SCHEME");
                }
                else {
                    this.set(path, "PATH");
                }
            }
        };
        /**
         * Append the provided path to this URL's existing path. If the provided path contains a query,
         * then it will be added to this URL as well.
         */
        URLBuilder.prototype.appendPath = function (path) {
            if (path) {
                var currentPath = this.getPath();
                if (currentPath) {
                    if (!currentPath.endsWith("/")) {
                        currentPath += "/";
                    }
                    if (path.startsWith("/")) {
                        path = path.substring(1);
                    }
                    path = currentPath + path;
                }
                this.set(path, "PATH");
            }
        };
        /**
         * Get the path that has been set in this URL.
         */
        URLBuilder.prototype.getPath = function () {
            return this._path;
        };
        /**
         * Set the query in this URL.
         */
        URLBuilder.prototype.setQuery = function (query) {
            if (!query) {
                this._query = undefined;
            }
            else {
                this._query = URLQuery.parse(query);
            }
        };
        /**
         * Set a query parameter with the provided name and value in this URL's query. If the provided
         * query parameter value is undefined or empty, then the query parameter will be removed if it
         * existed.
         */
        URLBuilder.prototype.setQueryParameter = function (queryParameterName, queryParameterValue) {
            if (queryParameterName) {
                if (!this._query) {
                    this._query = new URLQuery();
                }
                this._query.set(queryParameterName, queryParameterValue);
            }
        };
        /**
         * Get the value of the query parameter with the provided query parameter name. If no query
         * parameter exists with the provided name, then undefined will be returned.
         */
        URLBuilder.prototype.getQueryParameterValue = function (queryParameterName) {
            return this._query ? this._query.get(queryParameterName) : undefined;
        };
        /**
         * Get the query in this URL.
         */
        URLBuilder.prototype.getQuery = function () {
            return this._query ? this._query.toString() : undefined;
        };
        /**
         * Set the parts of this URL by parsing the provided text using the provided startState.
         */
        URLBuilder.prototype.set = function (text, startState) {
            var tokenizer = new URLTokenizer(text, startState);
            while (tokenizer.next()) {
                var token = tokenizer.current();
                if (token) {
                    switch (token.type) {
                        case "SCHEME":
                            this._scheme = token.text || undefined;
                            break;
                        case "HOST":
                            this._host = token.text || undefined;
                            break;
                        case "PORT":
                            this._port = token.text || undefined;
                            break;
                        case "PATH":
                            var tokenPath = token.text || undefined;
                            if (!this._path || this._path === "/" || tokenPath !== "/") {
                                this._path = tokenPath;
                            }
                            break;
                        case "QUERY":
                            this._query = URLQuery.parse(token.text);
                            break;
                        default:
                            throw new Error("Unrecognized URLTokenType: " + token.type);
                    }
                }
            }
        };
        URLBuilder.prototype.toString = function () {
            var result = "";
            if (this._scheme) {
                result += this._scheme + "://";
            }
            if (this._host) {
                result += this._host;
            }
            if (this._port) {
                result += ":" + this._port;
            }
            if (this._path) {
                if (!this._path.startsWith("/")) {
                    result += "/";
                }
                result += this._path;
            }
            if (this._query && this._query.any()) {
                result += "?" + this._query.toString();
            }
            return result;
        };
        /**
         * If the provided searchValue is found in this URLBuilder, then replace it with the provided
         * replaceValue.
         */
        URLBuilder.prototype.replaceAll = function (searchValue, replaceValue) {
            if (searchValue) {
                this.setScheme(replaceAll(this.getScheme(), searchValue, replaceValue));
                this.setHost(replaceAll(this.getHost(), searchValue, replaceValue));
                this.setPort(replaceAll(this.getPort(), searchValue, replaceValue));
                this.setPath(replaceAll(this.getPath(), searchValue, replaceValue));
                this.setQuery(replaceAll(this.getQuery(), searchValue, replaceValue));
            }
        };
        URLBuilder.parse = function (text) {
            var result = new URLBuilder();
            result.set(text, "SCHEME_OR_HOST");
            return result;
        };
        return URLBuilder;
    }());
    var URLToken = /** @class */ (function () {
        function URLToken(text, type) {
            this.text = text;
            this.type = type;
        }
        URLToken.scheme = function (text) {
            return new URLToken(text, "SCHEME");
        };
        URLToken.host = function (text) {
            return new URLToken(text, "HOST");
        };
        URLToken.port = function (text) {
            return new URLToken(text, "PORT");
        };
        URLToken.path = function (text) {
            return new URLToken(text, "PATH");
        };
        URLToken.query = function (text) {
            return new URLToken(text, "QUERY");
        };
        return URLToken;
    }());
    /**
     * Get whether or not the provided character (single character string) is an alphanumeric (letter or
     * digit) character.
     */
    function isAlphaNumericCharacter(character) {
        var characterCode = character.charCodeAt(0);
        return (48 /* '0' */ <= characterCode && characterCode <= 57 /* '9' */) ||
            (65 /* 'A' */ <= characterCode && characterCode <= 90 /* 'Z' */) ||
            (97 /* 'a' */ <= characterCode && characterCode <= 122 /* 'z' */);
    }
    /**
     * A class that tokenizes URL strings.
     */
    var URLTokenizer = /** @class */ (function () {
        function URLTokenizer(_text, state) {
            this._text = _text;
            this._textLength = _text ? _text.length : 0;
            this._currentState = state != undefined ? state : "SCHEME_OR_HOST";
            this._currentIndex = 0;
        }
        /**
         * Get the current URLToken this URLTokenizer is pointing at, or undefined if the URLTokenizer
         * hasn't started or has finished tokenizing.
         */
        URLTokenizer.prototype.current = function () {
            return this._currentToken;
        };
        /**
         * Advance to the next URLToken and return whether or not a URLToken was found.
         */
        URLTokenizer.prototype.next = function () {
            if (!hasCurrentCharacter(this)) {
                this._currentToken = undefined;
            }
            else {
                switch (this._currentState) {
                    case "SCHEME":
                        nextScheme(this);
                        break;
                    case "SCHEME_OR_HOST":
                        nextSchemeOrHost(this);
                        break;
                    case "HOST":
                        nextHost(this);
                        break;
                    case "PORT":
                        nextPort(this);
                        break;
                    case "PATH":
                        nextPath(this);
                        break;
                    case "QUERY":
                        nextQuery(this);
                        break;
                    default:
                        throw new Error("Unrecognized URLTokenizerState: " + this._currentState);
                }
            }
            return !!this._currentToken;
        };
        return URLTokenizer;
    }());
    /**
     * Read the remaining characters from this Tokenizer's character stream.
     */
    function readRemaining(tokenizer) {
        var result = "";
        if (tokenizer._currentIndex < tokenizer._textLength) {
            result = tokenizer._text.substring(tokenizer._currentIndex);
            tokenizer._currentIndex = tokenizer._textLength;
        }
        return result;
    }
    /**
     * Whether or not this URLTokenizer has a current character.
     */
    function hasCurrentCharacter(tokenizer) {
        return tokenizer._currentIndex < tokenizer._textLength;
    }
    /**
     * Get the character in the text string at the current index.
     */
    function getCurrentCharacter(tokenizer) {
        return tokenizer._text[tokenizer._currentIndex];
    }
    /**
     * Advance to the character in text that is "step" characters ahead. If no step value is provided,
     * then step will default to 1.
     */
    function nextCharacter(tokenizer, step) {
        if (hasCurrentCharacter(tokenizer)) {
            if (!step) {
                step = 1;
            }
            tokenizer._currentIndex += step;
        }
    }
    /**
     * Starting with the current character, peek "charactersToPeek" number of characters ahead in this
     * Tokenizer's stream of characters.
     */
    function peekCharacters(tokenizer, charactersToPeek) {
        var endIndex = tokenizer._currentIndex + charactersToPeek;
        if (tokenizer._textLength < endIndex) {
            endIndex = tokenizer._textLength;
        }
        return tokenizer._text.substring(tokenizer._currentIndex, endIndex);
    }
    /**
     * Read characters from this Tokenizer until the end of the stream or until the provided condition
     * is false when provided the current character.
     */
    function readWhile(tokenizer, condition) {
        var result = "";
        while (hasCurrentCharacter(tokenizer)) {
            var currentCharacter = getCurrentCharacter(tokenizer);
            if (!condition(currentCharacter)) {
                break;
            }
            else {
                result += currentCharacter;
                nextCharacter(tokenizer);
            }
        }
        return result;
    }
    /**
     * Read characters from this Tokenizer until a non-alphanumeric character or the end of the
     * character stream is reached.
     */
    function readWhileLetterOrDigit(tokenizer) {
        return readWhile(tokenizer, function (character) { return isAlphaNumericCharacter(character); });
    }
    /**
     * Read characters from this Tokenizer until one of the provided terminating characters is read or
     * the end of the character stream is reached.
     */
    function readUntilCharacter(tokenizer) {
        var terminatingCharacters = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            terminatingCharacters[_i - 1] = arguments[_i];
        }
        return readWhile(tokenizer, function (character) { return terminatingCharacters.indexOf(character) === -1; });
    }
    function nextScheme(tokenizer) {
        var scheme = readWhileLetterOrDigit(tokenizer);
        tokenizer._currentToken = URLToken.scheme(scheme);
        if (!hasCurrentCharacter(tokenizer)) {
            tokenizer._currentState = "DONE";
        }
        else {
            tokenizer._currentState = "HOST";
        }
    }
    function nextSchemeOrHost(tokenizer) {
        var schemeOrHost = readUntilCharacter(tokenizer, ":", "/", "?");
        if (!hasCurrentCharacter(tokenizer)) {
            tokenizer._currentToken = URLToken.host(schemeOrHost);
            tokenizer._currentState = "DONE";
        }
        else if (getCurrentCharacter(tokenizer) === ":") {
            if (peekCharacters(tokenizer, 3) === "://") {
                tokenizer._currentToken = URLToken.scheme(schemeOrHost);
                tokenizer._currentState = "HOST";
            }
            else {
                tokenizer._currentToken = URLToken.host(schemeOrHost);
                tokenizer._currentState = "PORT";
            }
        }
        else {
            tokenizer._currentToken = URLToken.host(schemeOrHost);
            if (getCurrentCharacter(tokenizer) === "/") {
                tokenizer._currentState = "PATH";
            }
            else {
                tokenizer._currentState = "QUERY";
            }
        }
    }
    function nextHost(tokenizer) {
        if (peekCharacters(tokenizer, 3) === "://") {
            nextCharacter(tokenizer, 3);
        }
        var host = readUntilCharacter(tokenizer, ":", "/", "?");
        tokenizer._currentToken = URLToken.host(host);
        if (!hasCurrentCharacter(tokenizer)) {
            tokenizer._currentState = "DONE";
        }
        else if (getCurrentCharacter(tokenizer) === ":") {
            tokenizer._currentState = "PORT";
        }
        else if (getCurrentCharacter(tokenizer) === "/") {
            tokenizer._currentState = "PATH";
        }
        else {
            tokenizer._currentState = "QUERY";
        }
    }
    function nextPort(tokenizer) {
        if (getCurrentCharacter(tokenizer) === ":") {
            nextCharacter(tokenizer);
        }
        var port = readUntilCharacter(tokenizer, "/", "?");
        tokenizer._currentToken = URLToken.port(port);
        if (!hasCurrentCharacter(tokenizer)) {
            tokenizer._currentState = "DONE";
        }
        else if (getCurrentCharacter(tokenizer) === "/") {
            tokenizer._currentState = "PATH";
        }
        else {
            tokenizer._currentState = "QUERY";
        }
    }
    function nextPath(tokenizer) {
        var path = readUntilCharacter(tokenizer, "?");
        tokenizer._currentToken = URLToken.path(path);
        if (!hasCurrentCharacter(tokenizer)) {
            tokenizer._currentState = "DONE";
        }
        else {
            tokenizer._currentState = "QUERY";
        }
    }
    function nextQuery(tokenizer) {
        if (getCurrentCharacter(tokenizer) === "?") {
            nextCharacter(tokenizer);
        }
        var query = readRemaining(tokenizer);
        tokenizer._currentToken = URLToken.query(query);
        tokenizer._currentState = "DONE";
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    function redirectPolicy(maximumRetries) {
        if (maximumRetries === void 0) { maximumRetries = 20; }
        return {
            create: function (nextPolicy, options) {
                return new RedirectPolicy(nextPolicy, options, maximumRetries);
            }
        };
    }
    var RedirectPolicy = /** @class */ (function (_super) {
        __extends(RedirectPolicy, _super);
        function RedirectPolicy(nextPolicy, options, maxRetries) {
            if (maxRetries === void 0) { maxRetries = 20; }
            var _this = _super.call(this, nextPolicy, options) || this;
            _this.maxRetries = maxRetries;
            return _this;
        }
        RedirectPolicy.prototype.sendRequest = function (request) {
            var _this = this;
            return this._nextPolicy.sendRequest(request).then(function (response) { return handleRedirect(_this, response, 0); });
        };
        return RedirectPolicy;
    }(BaseRequestPolicy));
    function handleRedirect(policy, response, currentRetries) {
        var request = response.request, status = response.status;
        var locationHeader = response.headers.get("location");
        if (locationHeader &&
            (status === 300 || status === 307 || (status === 303 && request.method === "POST")) &&
            (!policy.maxRetries || currentRetries < policy.maxRetries)) {
            var builder = URLBuilder.parse(request.url);
            builder.setPath(locationHeader);
            request.url = builder.toString();
            // POST request with Status code 303 should be converted into a
            // redirected GET request if the redirect url is present in the location header
            if (status === 303) {
                request.method = "GET";
            }
            return policy._nextPolicy.sendRequest(request)
                .then(function (res) { return handleRedirect(policy, res, currentRetries + 1); });
        }
        return Promise.resolve(response);
    }

    function rpRegistrationPolicy(retryTimeout) {
        if (retryTimeout === void 0) { retryTimeout = 30; }
        return {
            create: function (nextPolicy, options) {
                return new RPRegistrationPolicy(nextPolicy, options, retryTimeout);
            }
        };
    }
    var RPRegistrationPolicy = /** @class */ (function (_super) {
        __extends(RPRegistrationPolicy, _super);
        function RPRegistrationPolicy(nextPolicy, options, _retryTimeout) {
            if (_retryTimeout === void 0) { _retryTimeout = 30; }
            var _this = _super.call(this, nextPolicy, options) || this;
            _this._retryTimeout = _retryTimeout;
            return _this;
        }
        RPRegistrationPolicy.prototype.sendRequest = function (request) {
            var _this = this;
            return this._nextPolicy.sendRequest(request.clone())
                .then(function (response) { return registerIfNeeded(_this, request, response); });
        };
        return RPRegistrationPolicy;
    }(BaseRequestPolicy));
    function registerIfNeeded(policy, request, response) {
        if (response.status === 409) {
            var rpName = checkRPNotRegisteredError(response.bodyAsText);
            if (rpName) {
                var urlPrefix = extractSubscriptionUrl(request.url);
                return registerRP(policy, urlPrefix, rpName, request)
                    // Autoregistration of ${provider} failed for some reason. We will not return this error
                    // instead will return the initial response with 409 status code back to the user.
                    // do nothing here as we are returning the original response at the end of this method.
                    .catch(function () { return false; })
                    .then(function (registrationStatus) {
                    if (registrationStatus) {
                        // Retry the original request. We have to change the x-ms-client-request-id
                        // otherwise Azure endpoint will return the initial 409 (cached) response.
                        request.headers.set("x-ms-client-request-id", generateUuid());
                        return policy._nextPolicy.sendRequest(request.clone());
                    }
                    return response;
                });
            }
        }
        return Promise.resolve(response);
    }
    /**
     * Reuses the headers of the original request and url (if specified).
     * @param {WebResource} originalRequest The original request
     * @param {boolean} reuseUrlToo Should the url from the original request be reused as well. Default false.
     * @returns {object} A new request object with desired headers.
     */
    function getRequestEssentials(originalRequest, reuseUrlToo) {
        if (reuseUrlToo === void 0) { reuseUrlToo = false; }
        var reqOptions = originalRequest.clone();
        if (reuseUrlToo) {
            reqOptions.url = originalRequest.url;
        }
        // We have to change the x-ms-client-request-id otherwise Azure endpoint
        // will return the initial 409 (cached) response.
        reqOptions.headers.set("x-ms-client-request-id", generateUuid());
        // Set content-type to application/json
        reqOptions.headers.set("Content-Type", "application/json; charset=utf-8");
        return reqOptions;
    }
    /**
     * Validates the error code and message associated with 409 response status code. If it matches to that of
     * RP not registered then it returns the name of the RP else returns undefined.
     * @param {string} body The response body received after making the original request.
     * @returns {string} The name of the RP if condition is satisfied else undefined.
     */
    function checkRPNotRegisteredError(body) {
        var result, responseBody;
        if (body) {
            try {
                responseBody = JSON.parse(body);
            }
            catch (err) {
                // do nothing;
            }
            if (responseBody && responseBody.error && responseBody.error.message &&
                responseBody.error.code && responseBody.error.code === "MissingSubscriptionRegistration") {
                var matchRes = responseBody.error.message.match(/.*'(.*)'/i);
                if (matchRes) {
                    result = matchRes.pop();
                }
            }
        }
        return result;
    }
    /**
     * Extracts the first part of the URL, just after subscription:
     * https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/
     * @param {string} url The original request url
     * @returns {string} The url prefix as explained above.
     */
    function extractSubscriptionUrl(url) {
        var result;
        var matchRes = url.match(/.*\/subscriptions\/[a-f0-9-]+\//ig);
        if (matchRes && matchRes[0]) {
            result = matchRes[0];
        }
        else {
            throw new Error("Unable to extract subscriptionId from the given url - " + url + ".");
        }
        return result;
    }
    /**
     * Registers the given provider.
     * @param {RPRegistrationPolicy} policy The RPRegistrationPolicy this function is being called against.
     * @param {string} urlPrefix https://management.azure.com/subscriptions/00000000-0000-0000-0000-000000000000/
     * @param {string} provider The provider name to be registered.
     * @param {WebResource} originalRequest The original request sent by the user that returned a 409 response
     * with a message that the provider is not registered.
     * @param {registrationCallback} callback The callback that handles the RP registration
     */
    function registerRP(policy, urlPrefix, provider, originalRequest) {
        var postUrl = urlPrefix + "providers/" + provider + "/register?api-version=2016-02-01";
        var getUrl = urlPrefix + "providers/" + provider + "?api-version=2016-02-01";
        var reqOptions = getRequestEssentials(originalRequest);
        reqOptions.method = "POST";
        reqOptions.url = postUrl;
        return policy._nextPolicy.sendRequest(reqOptions)
            .then(function (response) {
            if (response.status !== 200) {
                throw new Error("Autoregistration of " + provider + " failed. Please try registering manually.");
            }
            return getRegistrationStatus(policy, getUrl, originalRequest);
        });
    }
    /**
     * Polls the registration status of the provider that was registered. Polling happens at an interval of 30 seconds.
     * Polling will happen till the registrationState property of the response body is "Registered".
     * @param {RPRegistrationPolicy} policy The RPRegistrationPolicy this function is being called against.
     * @param {string} url The request url for polling
     * @param {WebResource} originalRequest The original request sent by the user that returned a 409 response
     * with a message that the provider is not registered.
     * @returns {Promise<boolean>} True if RP Registration is successful.
     */
    function getRegistrationStatus(policy, url, originalRequest) {
        var reqOptions = getRequestEssentials(originalRequest);
        reqOptions.url = url;
        reqOptions.method = "GET";
        return policy._nextPolicy.sendRequest(reqOptions).then(function (res) {
            var obj = res.parsedBody;
            if (res.parsedBody && obj.registrationState && obj.registrationState === "Registered") {
                return true;
            }
            else {
                return delay(policy._retryTimeout * 1000).then(function () { return getRegistrationStatus(policy, url, originalRequest); });
            }
        });
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    function signingPolicy(authenticationProvider) {
        return {
            create: function (nextPolicy, options) {
                return new SigningPolicy(nextPolicy, options, authenticationProvider);
            }
        };
    }
    var SigningPolicy = /** @class */ (function (_super) {
        __extends(SigningPolicy, _super);
        function SigningPolicy(nextPolicy, options, authenticationProvider) {
            var _this = _super.call(this, nextPolicy, options) || this;
            _this.authenticationProvider = authenticationProvider;
            return _this;
        }
        SigningPolicy.prototype.signRequest = function (request) {
            return this.authenticationProvider.signRequest(request);
        };
        SigningPolicy.prototype.sendRequest = function (request) {
            var _this = this;
            return this.signRequest(request).then(function (nextRequest) { return _this._nextPolicy.sendRequest(nextRequest); });
        };
        return SigningPolicy;
    }(BaseRequestPolicy));

    // Copyright (c) Microsoft Corporation. All rights reserved.
    function systemErrorRetryPolicy(retryCount, retryInterval, minRetryInterval, maxRetryInterval) {
        return {
            create: function (nextPolicy, options) {
                return new SystemErrorRetryPolicy(nextPolicy, options, retryCount, retryInterval, minRetryInterval, maxRetryInterval);
            }
        };
    }
    /**
     * @class
     * Instantiates a new "ExponentialRetryPolicyFilter" instance.
     *
     * @constructor
     * @param {number} retryCount        The client retry count.
     * @param {number} retryInterval     The client retry interval, in milliseconds.
     * @param {number} minRetryInterval  The minimum retry interval, in milliseconds.
     * @param {number} maxRetryInterval  The maximum retry interval, in milliseconds.
     */
    var SystemErrorRetryPolicy = /** @class */ (function (_super) {
        __extends(SystemErrorRetryPolicy, _super);
        function SystemErrorRetryPolicy(nextPolicy, options, retryCount, retryInterval, minRetryInterval, maxRetryInterval) {
            var _this = _super.call(this, nextPolicy, options) || this;
            _this.DEFAULT_CLIENT_RETRY_INTERVAL = 1000 * 30;
            _this.DEFAULT_CLIENT_RETRY_COUNT = 3;
            _this.DEFAULT_CLIENT_MAX_RETRY_INTERVAL = 1000 * 90;
            _this.DEFAULT_CLIENT_MIN_RETRY_INTERVAL = 1000 * 3;
            _this.retryCount = typeof retryCount === "number" ? retryCount : _this.DEFAULT_CLIENT_RETRY_COUNT;
            _this.retryInterval = typeof retryInterval === "number" ? retryInterval : _this.DEFAULT_CLIENT_RETRY_INTERVAL;
            _this.minRetryInterval = typeof minRetryInterval === "number" ? minRetryInterval : _this.DEFAULT_CLIENT_MIN_RETRY_INTERVAL;
            _this.maxRetryInterval = typeof maxRetryInterval === "number" ? maxRetryInterval : _this.DEFAULT_CLIENT_MAX_RETRY_INTERVAL;
            return _this;
        }
        SystemErrorRetryPolicy.prototype.sendRequest = function (request) {
            var _this = this;
            return this._nextPolicy.sendRequest(request.clone()).then(function (response) { return retry$1(_this, request, response); });
        };
        return SystemErrorRetryPolicy;
    }(BaseRequestPolicy));
    /**
     * Determines if the operation should be retried and how long to wait until the next retry.
     *
     * @param {number} statusCode The HTTP status code.
     * @param {RetryData} retryData  The retry data.
     * @return {boolean} True if the operation qualifies for a retry; false otherwise.
     */
    function shouldRetry$1(policy, retryData) {
        var currentCount;
        if (!retryData) {
            throw new Error("retryData for the SystemErrorRetryPolicyFilter cannot be null.");
        }
        else {
            currentCount = (retryData && retryData.retryCount);
        }
        return (currentCount < policy.retryCount);
    }
    /**
     * Updates the retry data for the next attempt.
     *
     * @param {RetryData} retryData  The retry data.
     * @param {object} err        The operation"s error, if any.
     */
    function updateRetryData$1(policy, retryData, err) {
        if (!retryData) {
            retryData = {
                retryCount: 0,
                retryInterval: 0
            };
        }
        if (err) {
            if (retryData.error) {
                err.innerError = retryData.error;
            }
            retryData.error = err;
        }
        // Adjust retry count
        retryData.retryCount++;
        // Adjust retry interval
        var incrementDelta = Math.pow(2, retryData.retryCount) - 1;
        var boundedRandDelta = policy.retryInterval * 0.8 +
            Math.floor(Math.random() * (policy.retryInterval * 1.2 - policy.retryInterval * 0.8));
        incrementDelta *= boundedRandDelta;
        retryData.retryInterval = Math.min(policy.minRetryInterval + incrementDelta, policy.maxRetryInterval);
        return retryData;
    }
    function retry$1(policy, request, operationResponse, retryData, err) {
        retryData = updateRetryData$1(policy, retryData, err);
        if (err && err.code && shouldRetry$1(policy, retryData) &&
            (err.code === "ETIMEDOUT" || err.code === "ESOCKETTIMEDOUT" || err.code === "ECONNREFUSED" ||
                err.code === "ECONNRESET" || err.code === "ENOENT")) {
            // If previous operation ended with an error and the policy allows a retry, do that
            return delay(retryData.retryInterval)
                .then(function () { return policy._nextPolicy.sendRequest(request.clone()); })
                .then(function (res) { return retry$1(policy, request, res, retryData, err); })
                .catch(function (err) { return retry$1(policy, request, operationResponse, retryData, err); });
        }
        else {
            if (err != undefined) {
                // If the operation failed in the end, return all errors instead of just the last one
                err = retryData.error;
                return Promise.reject(err);
            }
            return Promise.resolve(operationResponse);
        }
    }

    // Copyright (c) Microsoft Corporation. All rights reserved.
    // Licensed under the MIT License. See License.txt in the project root for license information.
    /**
     * The format that will be used to join an array of values together for a query parameter value.
     */
    var QueryCollectionFormat;
    (function (QueryCollectionFormat) {
        QueryCollectionFormat["Csv"] = ",";
        QueryCollectionFormat["Ssv"] = " ";
        QueryCollectionFormat["Tsv"] = "\t";
        QueryCollectionFormat["Pipes"] = "|";
        QueryCollectionFormat["Multi"] = "Multi";
    })(QueryCollectionFormat || (QueryCollectionFormat = {}));

    // Copyright (c) Microsoft Corporation. All rights reserved.
    function loadEnvironmentProxyValue() {
        if (!process) {
            return undefined;
        }
        if (process.env[Constants.HTTPS_PROXY]) {
            return process.env[Constants.HTTPS_PROXY];
        }
        else if (process.env[Constants.HTTPS_PROXY.toLowerCase()]) {
            return process.env[Constants.HTTPS_PROXY.toLowerCase()];
        }
        else if (process.env[Constants.HTTP_PROXY]) {
            return process.env[Constants.HTTP_PROXY];
        }
        else if (process.env[Constants.HTTP_PROXY.toLowerCase()]) {
            return process.env[Constants.HTTP_PROXY.toLowerCase()];
        }
        return undefined;
    }
    function getDefaultProxySettings(proxyUrl) {
        if (!proxyUrl) {
            proxyUrl = loadEnvironmentProxyValue();
            if (!proxyUrl) {
                return undefined;
            }
        }
        var parsedUrl = URLBuilder.parse(proxyUrl);
        return {
            host: parsedUrl.getScheme() + "://" + parsedUrl.getHost(),
            port: Number.parseInt(parsedUrl.getPort() || "80")
        };
    }
    function proxyPolicy(proxySettings) {
        return {
            create: function (nextPolicy, options) {
                return new ProxyPolicy(nextPolicy, options, proxySettings);
            }
        };
    }
    var ProxyPolicy = /** @class */ (function (_super) {
        __extends(ProxyPolicy, _super);
        function ProxyPolicy(nextPolicy, options, proxySettings) {
            var _this = _super.call(this, nextPolicy, options) || this;
            _this.proxySettings = proxySettings;
            return _this;
        }
        ProxyPolicy.prototype.sendRequest = function (request) {
            if (!request.proxySettings) {
                request.proxySettings = this.proxySettings;
            }
            return this._nextPolicy.sendRequest(request);
        };
        return ProxyPolicy;
    }(BaseRequestPolicy));

    // Copyright (c) Microsoft Corporation. All rights reserved.
    var StatusCodes = Constants.HttpConstants.StatusCodes;
    function throttlingRetryPolicy() {
        return {
            create: function (nextPolicy, options) {
                return new ThrottlingRetryPolicy(nextPolicy, options);
            }
        };
    }
    /**
     * To learn more, please refer to
     * https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-manager-request-limits,
     * https://docs.microsoft.com/en-us/azure/azure-subscription-service-limits and
     * https://docs.microsoft.com/en-us/azure/virtual-machines/troubleshooting/troubleshooting-throttling-errors
     */
    var ThrottlingRetryPolicy = /** @class */ (function (_super) {
        __extends(ThrottlingRetryPolicy, _super);
        function ThrottlingRetryPolicy(nextPolicy, options, _handleResponse) {
            var _this = _super.call(this, nextPolicy, options) || this;
            _this._handleResponse = _handleResponse || _this._defaultResponseHandler;
            return _this;
        }
        ThrottlingRetryPolicy.prototype.sendRequest = function (httpRequest) {
            return __awaiter(this, void 0, void 0, function () {
                var _this = this;
                return __generator(this, function (_a) {
                    return [2 /*return*/, this._nextPolicy.sendRequest(httpRequest.clone()).then(function (response) {
                            if (response.status !== StatusCodes.TooManyRequests) {
                                return response;
                            }
                            else {
                                return _this._handleResponse(httpRequest, response);
                            }
                        })];
                });
            });
        };
        ThrottlingRetryPolicy.prototype._defaultResponseHandler = function (httpRequest, httpResponse) {
            return __awaiter(this, void 0, void 0, function () {
                var retryAfterHeader, delayInMs;
                var _this = this;
                return __generator(this, function (_a) {
                    retryAfterHeader = httpResponse.headers.get(Constants.HeaderConstants.RETRY_AFTER);
                    if (retryAfterHeader) {
                        delayInMs = ThrottlingRetryPolicy.parseRetryAfterHeader(retryAfterHeader);
                        if (delayInMs) {
                            return [2 /*return*/, delay(delayInMs).then(function (_) { return _this._nextPolicy.sendRequest(httpRequest); })];
                        }
                    }
                    return [2 /*return*/, httpResponse];
                });
            });
        };
        ThrottlingRetryPolicy.parseRetryAfterHeader = function (headerValue) {
            var retryAfterInSeconds = Number(headerValue);
            if (Number.isNaN(retryAfterInSeconds)) {
                return ThrottlingRetryPolicy.parseDateRetryAfterHeader(headerValue);
            }
            else {
                return retryAfterInSeconds * 1000;
            }
        };
        ThrottlingRetryPolicy.parseDateRetryAfterHeader = function (headerValue) {
            try {
                var now = Date.now();
                var date = Date.parse(headerValue);
                var diff = date - now;
                return Number.isNaN(diff) ? undefined : diff;
            }
            catch (error) {
                return undefined;
            }
        };
        return ThrottlingRetryPolicy;
    }(BaseRequestPolicy));

    // Copyright (c) Microsoft Corporation. All rights reserved.
    /**
     * @class
     * Initializes a new instance of the ServiceClient.
     */
    var ServiceClient = /** @class */ (function () {
        /**
         * The ServiceClient constructor
         * @constructor
         * @param {ServiceClientCredentials} [credentials] The credentials object used for authentication.
         * @param {ServiceClientOptions} [options] The service client options that govern the behavior of the client.
         */
        function ServiceClient(credentials, options) {
            if (!options) {
                options = {};
            }
            if (credentials && !credentials.signRequest) {
                throw new Error("credentials argument needs to implement signRequest method");
            }
            this._withCredentials = options.withCredentials || false;
            this._httpClient = options.httpClient || new XhrHttpClient();
            this._requestPolicyOptions = new RequestPolicyOptions(options.httpPipelineLogger);
            var requestPolicyFactories;
            if (Array.isArray(options.requestPolicyFactories)) {
                requestPolicyFactories = options.requestPolicyFactories;
            }
            else {
                requestPolicyFactories = createDefaultRequestPolicyFactories(credentials, options);
                if (options.requestPolicyFactories) {
                    var newRequestPolicyFactories = options.requestPolicyFactories(requestPolicyFactories);
                    if (newRequestPolicyFactories) {
                        requestPolicyFactories = newRequestPolicyFactories;
                    }
                }
            }
            this._requestPolicyFactories = requestPolicyFactories;
        }
        /**
         * Send the provided httpRequest.
         */
        ServiceClient.prototype.sendRequest = function (options) {
            if (options === null || options === undefined || typeof options !== "object") {
                throw new Error("options cannot be null or undefined and it must be of type object.");
            }
            var httpRequest;
            try {
                if (options instanceof WebResource) {
                    options.validateRequestProperties();
                    httpRequest = options;
                }
                else {
                    httpRequest = new WebResource();
                    httpRequest = httpRequest.prepare(options);
                }
            }
            catch (error) {
                return Promise.reject(error);
            }
            var httpPipeline = this._httpClient;
            if (this._requestPolicyFactories && this._requestPolicyFactories.length > 0) {
                for (var i = this._requestPolicyFactories.length - 1; i >= 0; --i) {
                    httpPipeline = this._requestPolicyFactories[i].create(httpPipeline, this._requestPolicyOptions);
                }
            }
            return httpPipeline.sendRequest(httpRequest);
        };
        /**
         * Send an HTTP request that is populated using the provided OperationSpec.
         * @param {OperationArguments} operationArguments The arguments that the HTTP request's templated values will be populated from.
         * @param {OperationSpec} operationSpec The OperationSpec to use to populate the httpRequest.
         * @param {ServiceCallback} callback The callback to call when the response is received.
         */
        ServiceClient.prototype.sendOperationRequest = function (operationArguments, operationSpec, callback) {
            if (typeof operationArguments.options === "function") {
                callback = operationArguments.options;
                operationArguments.options = undefined;
            }
            var httpRequest = new WebResource();
            var result;
            try {
                var baseUri = operationSpec.baseUrl || this.baseUri;
                if (!baseUri) {
                    throw new Error("If operationSpec.baseUrl is not specified, then the ServiceClient must have a baseUri string property that contains the base URL to use.");
                }
                httpRequest.method = operationSpec.httpMethod;
                httpRequest.operationSpec = operationSpec;
                var requestUrl = URLBuilder.parse(baseUri);
                if (operationSpec.path) {
                    requestUrl.appendPath(operationSpec.path);
                }
                if (operationSpec.urlParameters && operationSpec.urlParameters.length > 0) {
                    for (var _i = 0, _a = operationSpec.urlParameters; _i < _a.length; _i++) {
                        var urlParameter = _a[_i];
                        var urlParameterValue = getOperationArgumentValueFromParameter(this, operationArguments, urlParameter, operationSpec.serializer);
                        urlParameterValue = operationSpec.serializer.serialize(urlParameter.mapper, urlParameterValue, getPathStringFromParameter(urlParameter));
                        if (!urlParameter.skipEncoding) {
                            urlParameterValue = encodeURIComponent(urlParameterValue);
                        }
                        requestUrl.replaceAll("{" + (urlParameter.mapper.serializedName || getPathStringFromParameter(urlParameter)) + "}", urlParameterValue);
                    }
                }
                if (operationSpec.queryParameters && operationSpec.queryParameters.length > 0) {
                    for (var _b = 0, _c = operationSpec.queryParameters; _b < _c.length; _b++) {
                        var queryParameter = _c[_b];
                        var queryParameterValue = getOperationArgumentValueFromParameter(this, operationArguments, queryParameter, operationSpec.serializer);
                        if (queryParameterValue != undefined) {
                            queryParameterValue = operationSpec.serializer.serialize(queryParameter.mapper, queryParameterValue, getPathStringFromParameter(queryParameter));
                            if (queryParameter.collectionFormat != undefined) {
                                if (queryParameter.collectionFormat === QueryCollectionFormat.Multi) {
                                    if (queryParameterValue.length === 0) {
                                        queryParameterValue = "";
                                    }
                                    else {
                                        for (var index in queryParameterValue) {
                                            var item = queryParameterValue[index];
                                            queryParameterValue[index] = item == undefined ? "" : item.toString();
                                        }
                                    }
                                }
                                else {
                                    queryParameterValue = queryParameterValue.join(queryParameter.collectionFormat);
                                }
                            }
                            if (!queryParameter.skipEncoding) {
                                if (Array.isArray(queryParameterValue)) {
                                    for (var index in queryParameterValue) {
                                        queryParameterValue[index] = encodeURIComponent(queryParameterValue[index]);
                                    }
                                }
                                else {
                                    queryParameterValue = encodeURIComponent(queryParameterValue);
                                }
                            }
                            requestUrl.setQueryParameter(queryParameter.mapper.serializedName || getPathStringFromParameter(queryParameter), queryParameterValue);
                        }
                    }
                }
                httpRequest.url = requestUrl.toString();
                var contentType = operationSpec.contentType || this.requestContentType;
                if (contentType) {
                    httpRequest.headers.set("Content-Type", contentType);
                }
                if (operationSpec.headerParameters) {
                    for (var _d = 0, _e = operationSpec.headerParameters; _d < _e.length; _d++) {
                        var headerParameter = _e[_d];
                        var headerValue = getOperationArgumentValueFromParameter(this, operationArguments, headerParameter, operationSpec.serializer);
                        if (headerValue != undefined) {
                            headerValue = operationSpec.serializer.serialize(headerParameter.mapper, headerValue, getPathStringFromParameter(headerParameter));
                            var headerCollectionPrefix = headerParameter.mapper.headerCollectionPrefix;
                            if (headerCollectionPrefix) {
                                for (var _f = 0, _g = Object.keys(headerValue); _f < _g.length; _f++) {
                                    var key = _g[_f];
                                    httpRequest.headers.set(headerCollectionPrefix + key, headerValue[key]);
                                }
                            }
                            else {
                                httpRequest.headers.set(headerParameter.mapper.serializedName || getPathStringFromParameter(headerParameter), headerValue);
                            }
                        }
                    }
                }
                var options = operationArguments.options;
                if (options) {
                    if (options.customHeaders) {
                        for (var customHeaderName in options.customHeaders) {
                            httpRequest.headers.set(customHeaderName, options.customHeaders[customHeaderName]);
                        }
                    }
                    if (options.abortSignal) {
                        httpRequest.abortSignal = options.abortSignal;
                    }
                    if (options.timeout) {
                        httpRequest.timeout = options.timeout;
                    }
                    if (options.onUploadProgress) {
                        httpRequest.onUploadProgress = options.onUploadProgress;
                    }
                    if (options.onDownloadProgress) {
                        httpRequest.onDownloadProgress = options.onDownloadProgress;
                    }
                }
                httpRequest.withCredentials = this._withCredentials;
                serializeRequestBody(this, httpRequest, operationArguments, operationSpec);
                if (httpRequest.streamResponseBody == undefined) {
                    httpRequest.streamResponseBody = isStreamOperation(operationSpec);
                }
                result = this.sendRequest(httpRequest)
                    .then(function (res) { return flattenResponse(res, operationSpec.responses[res.status]); });
            }
            catch (error) {
                result = Promise.reject(error);
            }
            var cb = callback;
            if (cb) {
                result
                    // tslint:disable-next-line:no-null-keyword
                    .then(function (res) { return cb(null, res._response.parsedBody, res._response.request, res._response); })
                    .catch(function (err) { return cb(err); });
            }
            return result;
        };
        return ServiceClient;
    }());
    function serializeRequestBody(serviceClient, httpRequest, operationArguments, operationSpec) {
        if (operationSpec.requestBody && operationSpec.requestBody.mapper) {
            httpRequest.body = getOperationArgumentValueFromParameter(serviceClient, operationArguments, operationSpec.requestBody, operationSpec.serializer);
            var bodyMapper = operationSpec.requestBody.mapper;
            var required = bodyMapper.required, xmlName = bodyMapper.xmlName, xmlElementName = bodyMapper.xmlElementName, serializedName = bodyMapper.serializedName;
            var typeName = bodyMapper.type.name;
            try {
                if (httpRequest.body != undefined || required) {
                    var requestBodyParameterPathString = getPathStringFromParameter(operationSpec.requestBody);
                    httpRequest.body = operationSpec.serializer.serialize(bodyMapper, httpRequest.body, requestBodyParameterPathString);
                    var isStream = typeName === MapperType.Stream;
                    if (operationSpec.isXML) {
                        if (typeName === MapperType.Sequence) {
                            httpRequest.body = stringifyXML(prepareXMLRootList(httpRequest.body, xmlElementName || xmlName || serializedName), { rootName: xmlName || serializedName });
                        }
                        else if (!isStream) {
                            httpRequest.body = stringifyXML(httpRequest.body, { rootName: xmlName || serializedName });
                        }
                    }
                    else if (!isStream) {
                        httpRequest.body = JSON.stringify(httpRequest.body);
                    }
                }
            }
            catch (error) {
                throw new Error("Error \"" + error.message + "\" occurred in serializing the payload - " + JSON.stringify(serializedName, undefined, "  ") + ".");
            }
        }
        else if (operationSpec.formDataParameters && operationSpec.formDataParameters.length > 0) {
            httpRequest.formData = {};
            for (var _i = 0, _a = operationSpec.formDataParameters; _i < _a.length; _i++) {
                var formDataParameter = _a[_i];
                var formDataParameterValue = getOperationArgumentValueFromParameter(serviceClient, operationArguments, formDataParameter, operationSpec.serializer);
                if (formDataParameterValue != undefined) {
                    var formDataParameterPropertyName = formDataParameter.mapper.serializedName || getPathStringFromParameter(formDataParameter);
                    httpRequest.formData[formDataParameterPropertyName] = operationSpec.serializer.serialize(formDataParameter.mapper, formDataParameterValue, getPathStringFromParameter(formDataParameter));
                }
            }
        }
    }
    function isRequestPolicyFactory(instance) {
        return typeof instance.create === "function";
    }
    function getValueOrFunctionResult(value, defaultValueCreator) {
        var result;
        if (typeof value === "string") {
            result = value;
        }
        else {
            result = defaultValueCreator();
            if (typeof value === "function") {
                result = value(result);
            }
        }
        return result;
    }
    function createDefaultRequestPolicyFactories(credentials, options) {
        var factories = [];
        if (options.generateClientRequestIdHeader) {
            factories.push(generateClientRequestIdPolicy(options.clientRequestIdHeaderName));
        }
        if (credentials) {
            if (isRequestPolicyFactory(credentials)) {
                factories.push(credentials);
            }
            else {
                factories.push(signingPolicy(credentials));
            }
        }
        var userAgentHeaderName = getValueOrFunctionResult(options.userAgentHeaderName, getDefaultUserAgentHeaderName);
        var userAgentHeaderValue = getValueOrFunctionResult(options.userAgent, getDefaultUserAgentValue);
        if (userAgentHeaderName && userAgentHeaderValue) {
            factories.push(userAgentPolicy({ key: userAgentHeaderName, value: userAgentHeaderValue }));
        }
        factories.push(redirectPolicy());
        factories.push(rpRegistrationPolicy(options.rpRegistrationRetryTimeout));
        if (!options.noRetryPolicy) {
            factories.push(exponentialRetryPolicy());
            factories.push(systemErrorRetryPolicy());
            factories.push(throttlingRetryPolicy());
        }
        factories.push(deserializationPolicy(options.deserializationContentTypes));
        var proxySettings = options.proxySettings || getDefaultProxySettings();
        if (proxySettings) {
            factories.push(proxyPolicy(proxySettings));
        }
        return factories;
    }
    function getOperationArgumentValueFromParameter(serviceClient, operationArguments, parameter, serializer) {
        return getOperationArgumentValueFromParameterPath(serviceClient, operationArguments, parameter.parameterPath, parameter.mapper, serializer);
    }
    function getOperationArgumentValueFromParameterPath(serviceClient, operationArguments, parameterPath, parameterMapper, serializer) {
        var value;
        if (typeof parameterPath === "string") {
            parameterPath = [parameterPath];
        }
        if (Array.isArray(parameterPath)) {
            if (parameterPath.length > 0) {
                if (parameterMapper.isConstant) {
                    value = parameterMapper.defaultValue;
                }
                else {
                    var propertySearchResult = getPropertyFromParameterPath(operationArguments, parameterPath);
                    if (!propertySearchResult.propertyFound) {
                        propertySearchResult = getPropertyFromParameterPath(serviceClient, parameterPath);
                    }
                    var useDefaultValue = false;
                    if (!propertySearchResult.propertyFound) {
                        useDefaultValue = parameterMapper.required || (parameterPath[0] === "options" && parameterPath.length === 2);
                    }
                    value = useDefaultValue ? parameterMapper.defaultValue : propertySearchResult.propertyValue;
                }
                // Serialize just for validation purposes.
                var parameterPathString = getPathStringFromParameterPath(parameterPath, parameterMapper);
                serializer.serialize(parameterMapper, value, parameterPathString);
            }
        }
        else {
            if (parameterMapper.required) {
                value = {};
            }
            for (var propertyName in parameterPath) {
                var propertyMapper = parameterMapper.type.modelProperties[propertyName];
                var propertyPath = parameterPath[propertyName];
                var propertyValue = getOperationArgumentValueFromParameterPath(serviceClient, operationArguments, propertyPath, propertyMapper, serializer);
                // Serialize just for validation purposes.
                var propertyPathString = getPathStringFromParameterPath(propertyPath, propertyMapper);
                serializer.serialize(propertyMapper, propertyValue, propertyPathString);
                if (propertyValue !== undefined) {
                    if (!value) {
                        value = {};
                    }
                    value[propertyName] = propertyValue;
                }
            }
        }
        return value;
    }
    function getPropertyFromParameterPath(parent, parameterPath) {
        var result = { propertyFound: false };
        var i = 0;
        for (; i < parameterPath.length; ++i) {
            var parameterPathPart = parameterPath[i];
            // Make sure to check inherited properties too, so don't use hasOwnProperty().
            if (parent != undefined && parameterPathPart in parent) {
                parent = parent[parameterPathPart];
            }
            else {
                break;
            }
        }
        if (i === parameterPath.length) {
            result.propertyValue = parent;
            result.propertyFound = true;
        }
        return result;
    }
    function flattenResponse(_response, responseSpec) {
        var parsedHeaders = _response.parsedHeaders;
        var bodyMapper = responseSpec && responseSpec.bodyMapper;
        var addOperationResponse = function (obj) {
            return Object.defineProperty(obj, "_response", {
                value: _response
            });
        };
        if (bodyMapper) {
            var typeName = bodyMapper.type.name;
            if (typeName === "Stream") {
                return addOperationResponse(__assign({}, parsedHeaders, { blobBody: _response.blobBody, readableStreamBody: _response.readableStreamBody }));
            }
            var modelProperties_1 = typeName === "Composite" && bodyMapper.type.modelProperties || {};
            var isPageableResponse = Object.keys(modelProperties_1).some(function (k) { return modelProperties_1[k].serializedName === ""; });
            if (typeName === "Sequence" || isPageableResponse) {
                var arrayResponse = (_response.parsedBody || []).slice();
                for (var _i = 0, _a = Object.keys(modelProperties_1); _i < _a.length; _i++) {
                    var key = _a[_i];
                    if (modelProperties_1[key].serializedName) {
                        arrayResponse[key] = _response.parsedBody[key];
                    }
                }
                if (parsedHeaders) {
                    for (var _b = 0, _c = Object.keys(parsedHeaders); _b < _c.length; _b++) {
                        var key = _c[_b];
                        arrayResponse[key] = parsedHeaders[key];
                    }
                }
                addOperationResponse(arrayResponse);
                return arrayResponse;
            }
            if (typeName === "Composite" || typeName === "Dictionary") {
                return addOperationResponse(__assign({}, parsedHeaders, _response.parsedBody));
            }
        }
        if (bodyMapper || _response.request.method === "HEAD" || isPrimitiveType(_response.parsedBody)) {
            // primitive body types and HEAD booleans
            return addOperationResponse(__assign({}, parsedHeaders, { body: _response.parsedBody }));
        }
        return addOperationResponse(__assign({}, parsedHeaders, _response.parsedBody));
    }

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is regenerated.
     */

    var index = /*#__PURE__*/Object.freeze({

    });

    /**
     * An aborter instance implements AbortSignal interface, can abort HTTP requests.
     *
     * - Call Aborter.none to create a new Aborter instance without timeout.
     * - Call Aborter.timeout() to create a new Aborter instance with timeout.
     *
     * For an existing instance aborter:
     * - Call aborter.withTimeout() to create and return a child Aborter instance with timeout.
     * - Call aborter.withValue(key, value) to create and return a child Aborter instance with key/value pair.
     * - Call aborter.abort() to abort current instance and all children instances.
     * - Call aborter.getValue(key) to search and get value with corresponding key from current aborter to all parents.
     *
     * @example
     * // Abort without timeout
     * await blockBlobURL.upload(Aborter.none, buf, buf.length);
     *
     * @example
     * // Abort container create in 1000ms
     * await blockBlobURL.upload(Aborter.timeout(1000), buf, buf.length);
     *
     * @example
     * // Share aborter cross multiple operations in 30s
     * // Upload the same data to 2 different data centers at the same time, abort another when any of them is finished
     * const aborter = Aborter.timeout(30 * 1000);
     * blockBlobURL1.upload(aborter, buf, buf.length).then(aborter.abort);
     * blockBlobURL2.upload(aborter, buf, buf.length).then(aborter.abort);
     *
     * @example
     * // Cascaded aborting
     * // All operations can't take more than 30 seconds
     * const aborter = Aborter.timeout(30 * 1000);
     *
     * // Following 2 operations can't take more than 25 seconds
     * await blockBlobURL.upload(aborter.withTimeout(25 * 1000), buf, buf.length);
     * await blockBlobURL.upload(aborter.withTimeout(25 * 1000), buf, buf.length);
     *
     * @export
     * @class Aborter
     * @implements {AbortSignalLike}
     */
    var Aborter = /** @class */ (function () {
        // private disposed: boolean = false;
        /**
         * Private constructor for internal usage, creates an instance of Aborter.
         *
         * @param {Aborter} [parent] Optional. Parent aborter.
         * @param {number} [timeout=0] Optional. Timeout before abort in millisecond, 0 means no timeout.
         * @param {string} [key] Optional. Immutable key in string.
         * @param {(string | number | boolean | null)} [value] Optional. Immutable value.
         * @memberof Aborter
         */
        function Aborter(parent, timeout, key, value) {
            var _this = this;
            if (timeout === void 0) { timeout = 0; }
            /**
             * onabort event listener.
             *
             * @memberof Aborter
             */
            this.onabort = null;
            // tslint:disable-next-line:variable-name
            this._aborted = false;
            this.children = []; // When child object calls dispose(), remove child from here
            this.abortEventListeners = [];
            this.parent = parent;
            this.key = key;
            this.value = value;
            if (timeout > 0) {
                this.timer = setTimeout(function () {
                    _this.abort.call(_this);
                }, timeout);
                // When called, the active Timeout object will not require the Node.js event loop
                // to remain active. If there is no other activity keeping the event loop running,
                // the process may exit before the Timeout object's callback is invoked.
                if (this.timer && isNode) {
                    this.timer.unref();
                }
            }
        }
        Object.defineProperty(Aborter.prototype, "aborted", {
            /**
             * Status of whether aborted or not.
             *
             * @readonly
             * @type {boolean}
             * @memberof Aborter
             */
            get: function () {
                return this._aborted;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(Aborter, "none", {
            /**
             * Creates a new Aborter instance without timeout.
             *
             * @readonly
             * @static
             * @type {Aborter}
             * @memberof Aborter
             */
            get: function () {
                return new Aborter(undefined, 0);
            },
            enumerable: true,
            configurable: true
        });
        /**
         * Creates a new Aborter instance with timeout in milliseconds.
         * Set parameter timeout to 0 will not create a timer.
         *
         * @static
         * @param {number} {timeout} in milliseconds
         * @returns {Aborter}
         * @memberof Aborter
         */
        Aborter.timeout = function (timeout) {
            return new Aborter(undefined, timeout);
        };
        /**
         * Create and return a new Aborter instance, which will be appended as a child node of current Aborter.
         * Current Aborter instance becomes father node of the new instance. When current or father Aborter node
         * triggers timeout event, all children nodes abort event will be triggered too.
         *
         * When timeout parameter (in millisecond) is larger than 0, the abort event will be triggered when timeout.
         * Otherwise, call abort() method to manually abort.
         *
         * @param {number} {timeout} Timeout in millisecond.
         * @returns {Aborter} The new Aborter instance created.
         * @memberof Aborter
         */
        Aborter.prototype.withTimeout = function (timeout) {
            var childCancelContext = new Aborter(this, timeout);
            this.children.push(childCancelContext);
            return childCancelContext;
        };
        /**
         * Create and return a new Aborter instance, which will be appended as a child node of current Aborter.
         * Current Aborter instance becomes father node of the new instance. When current or father Aborter node
         * triggers timeout event, all children nodes abort event will be triggered too.
         *
         * Immutable key value pair will be set into the new created Aborter instance.
         * Call getValue() to find out latest value with corresponding key in the chain of
         * [current node] -> [parent node] and [grand parent node]....
         *
         * @param {string} key
         * @param {(string | number | boolean | null)} [value]
         * @returns {Aborter}
         * @memberof Aborter
         */
        Aborter.prototype.withValue = function (key, value) {
            var childCancelContext = new Aborter(this, 0, key, value);
            this.children.push(childCancelContext);
            return childCancelContext;
        };
        /**
         * Find out latest value with corresponding key in the chain of
         * [current node] -> [parent node] -> [grand parent node] -> ... -> [root node].
         *
         * If key is not found, undefined will be returned.
         *
         * @param {string} key
         * @returns {(string | number | boolean | null | undefined)}
         * @memberof Aborter
         */
        Aborter.prototype.getValue = function (key) {
            for (var parent_1 = this; parent_1; parent_1 = parent_1.parent) {
                if (parent_1.key === key) {
                    return parent_1.value;
                }
            }
            return undefined;
        };
        /**
         * Trigger abort event immediately, the onabort and all abort event listeners will be triggered.
         * Will try to trigger abort event for all children Aborter nodes.
         *
         * - If there is a timeout, the timer will be cancelled.
         * - If aborted is true, nothing will happen.
         *
         * @returns
         * @memberof Aborter
         */
        Aborter.prototype.abort = function () {
            var _this = this;
            if (this.aborted) {
                return;
            }
            this.cancelTimer();
            if (this.onabort) {
                this.onabort.call(this, { type: "abort" });
            }
            this.abortEventListeners.forEach(function (listener) {
                listener.call(_this, { type: "abort" });
            });
            this.children.forEach(function (child) { return child.cancelByParent(); });
            this._aborted = true;
        };
        // public dispose() {
        //   if (this.disposed || this.aborted) {
        //     return;
        //   }
        //   this.cancelTimer();
        //   // (parent)A <- B <- C(child), if B disposes, when A abort, C will not abort
        //   if (this.parent) {
        //     const index = this.parent.children.indexOf(this);
        //     if (index > -1) {
        //       this.parent.children.splice(index, 1);
        //     }
        //   }
        //   this.disposed = true;
        // }
        /**
         * Added new "abort" event listener, only support "abort" event.
         *
         * @param {"abort"} _type Only support "abort" event
         * @param {(this: AbortSignalLike, ev: any) => any} listener
         * @memberof Aborter
         */
        Aborter.prototype.addEventListener = function (
        // tslint:disable-next-line:variable-name
        _type, listener) {
            this.abortEventListeners.push(listener);
        };
        /**
         * Remove "abort" event listener, only support "abort" event.
         *
         * @param {"abort"} _type Only support "abort" event
         * @param {(this: AbortSignalLike, ev: any) => any} listener
         * @memberof Aborter
         */
        Aborter.prototype.removeEventListener = function (
        // tslint:disable-next-line:variable-name
        _type, listener) {
            var index = this.abortEventListeners.indexOf(listener);
            if (index > -1) {
                this.abortEventListeners.splice(index, 1);
            }
        };
        Aborter.prototype.dispatchEvent = function () {
            throw new Error("Method not implemented.");
        };
        Aborter.prototype.cancelByParent = function () {
            // if (!this.disposed) {
            this.abort();
            // }
        };
        Aborter.prototype.cancelTimer = function () {
            if (this.timer) {
                clearTimeout(this.timer);
            }
        };
        return Aborter;
    }());

    // This file is used as a shim of "BlobDownloadResponse" for some browser bundlers
    // when trying to bundle "BlobDownloadResponse"
    // "BlobDownloadResponse" class is only available in Node.js runtime
    var BlobDownloadResponse = 1;

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is regenerated.
     */
    var KeyInfo = {
        serializedName: "KeyInfo",
        type: {
            name: "Composite",
            className: "KeyInfo",
            modelProperties: {
                start: {
                    xmlName: "Start",
                    required: true,
                    serializedName: "Start",
                    type: {
                        name: "String"
                    }
                },
                expiry: {
                    xmlName: "Expiry",
                    required: true,
                    serializedName: "Expiry",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var UserDelegationKey = {
        serializedName: "UserDelegationKey",
        type: {
            name: "Composite",
            className: "UserDelegationKey",
            modelProperties: {
                signedOid: {
                    xmlName: "SignedOid",
                    required: true,
                    serializedName: "SignedOid",
                    type: {
                        name: "String"
                    }
                },
                signedTid: {
                    xmlName: "SignedTid",
                    required: true,
                    serializedName: "SignedTid",
                    type: {
                        name: "String"
                    }
                },
                signedStart: {
                    xmlName: "SignedStart",
                    required: true,
                    serializedName: "SignedStart",
                    type: {
                        name: "String"
                    }
                },
                signedExpiry: {
                    xmlName: "SignedExpiry",
                    required: true,
                    serializedName: "SignedExpiry",
                    type: {
                        name: "String"
                    }
                },
                signedService: {
                    xmlName: "SignedService",
                    required: true,
                    serializedName: "SignedService",
                    type: {
                        name: "String"
                    }
                },
                signedVersion: {
                    xmlName: "SignedVersion",
                    required: true,
                    serializedName: "SignedVersion",
                    type: {
                        name: "String"
                    }
                },
                value: {
                    xmlName: "Value",
                    required: true,
                    serializedName: "Value",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var StorageError = {
        serializedName: "StorageError",
        type: {
            name: "Composite",
            className: "StorageError",
            modelProperties: {
                message: {
                    xmlName: "Message",
                    serializedName: "Message",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var DataLakeStorageErrorError = {
        serializedName: "DataLakeStorageError_error",
        type: {
            name: "Composite",
            className: "DataLakeStorageErrorError",
            modelProperties: {
                code: {
                    xmlName: "Code",
                    serializedName: "Code",
                    type: {
                        name: "String"
                    }
                },
                message: {
                    xmlName: "Message",
                    serializedName: "Message",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var DataLakeStorageError = {
        serializedName: "DataLakeStorageError",
        type: {
            name: "Composite",
            className: "DataLakeStorageError",
            modelProperties: {
                error: {
                    xmlName: "error",
                    serializedName: "error",
                    type: {
                        name: "Composite",
                        className: "DataLakeStorageErrorError"
                    }
                }
            }
        }
    };
    var AccessPolicy = {
        serializedName: "AccessPolicy",
        type: {
            name: "Composite",
            className: "AccessPolicy",
            modelProperties: {
                start: {
                    xmlName: "Start",
                    required: true,
                    serializedName: "Start",
                    type: {
                        name: "String"
                    }
                },
                expiry: {
                    xmlName: "Expiry",
                    required: true,
                    serializedName: "Expiry",
                    type: {
                        name: "String"
                    }
                },
                permission: {
                    xmlName: "Permission",
                    required: true,
                    serializedName: "Permission",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobProperties = {
        xmlName: "Properties",
        serializedName: "BlobProperties",
        type: {
            name: "Composite",
            className: "BlobProperties",
            modelProperties: {
                creationTime: {
                    xmlName: "Creation-Time",
                    serializedName: "Creation-Time",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                lastModified: {
                    xmlName: "Last-Modified",
                    required: true,
                    serializedName: "Last-Modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                etag: {
                    xmlName: "Etag",
                    required: true,
                    serializedName: "Etag",
                    type: {
                        name: "String"
                    }
                },
                contentLength: {
                    xmlName: "Content-Length",
                    serializedName: "Content-Length",
                    type: {
                        name: "Number"
                    }
                },
                contentType: {
                    xmlName: "Content-Type",
                    serializedName: "Content-Type",
                    type: {
                        name: "String"
                    }
                },
                contentEncoding: {
                    xmlName: "Content-Encoding",
                    serializedName: "Content-Encoding",
                    type: {
                        name: "String"
                    }
                },
                contentLanguage: {
                    xmlName: "Content-Language",
                    serializedName: "Content-Language",
                    type: {
                        name: "String"
                    }
                },
                contentMD5: {
                    xmlName: "Content-MD5",
                    serializedName: "Content-MD5",
                    type: {
                        name: "ByteArray"
                    }
                },
                contentDisposition: {
                    xmlName: "Content-Disposition",
                    serializedName: "Content-Disposition",
                    type: {
                        name: "String"
                    }
                },
                cacheControl: {
                    xmlName: "Cache-Control",
                    serializedName: "Cache-Control",
                    type: {
                        name: "String"
                    }
                },
                blobSequenceNumber: {
                    xmlName: "x-ms-blob-sequence-number",
                    serializedName: "x-ms-blob-sequence-number",
                    type: {
                        name: "Number"
                    }
                },
                blobType: {
                    xmlName: "BlobType",
                    serializedName: "BlobType",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "BlockBlob",
                            "PageBlob",
                            "AppendBlob"
                        ]
                    }
                },
                leaseStatus: {
                    xmlName: "LeaseStatus",
                    serializedName: "LeaseStatus",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "locked",
                            "unlocked"
                        ]
                    }
                },
                leaseState: {
                    xmlName: "LeaseState",
                    serializedName: "LeaseState",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "available",
                            "leased",
                            "expired",
                            "breaking",
                            "broken"
                        ]
                    }
                },
                leaseDuration: {
                    xmlName: "LeaseDuration",
                    serializedName: "LeaseDuration",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "infinite",
                            "fixed"
                        ]
                    }
                },
                copyId: {
                    xmlName: "CopyId",
                    serializedName: "CopyId",
                    type: {
                        name: "String"
                    }
                },
                copyStatus: {
                    xmlName: "CopyStatus",
                    serializedName: "CopyStatus",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "pending",
                            "success",
                            "aborted",
                            "failed"
                        ]
                    }
                },
                copySource: {
                    xmlName: "CopySource",
                    serializedName: "CopySource",
                    type: {
                        name: "String"
                    }
                },
                copyProgress: {
                    xmlName: "CopyProgress",
                    serializedName: "CopyProgress",
                    type: {
                        name: "String"
                    }
                },
                copyCompletionTime: {
                    xmlName: "CopyCompletionTime",
                    serializedName: "CopyCompletionTime",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                copyStatusDescription: {
                    xmlName: "CopyStatusDescription",
                    serializedName: "CopyStatusDescription",
                    type: {
                        name: "String"
                    }
                },
                serverEncrypted: {
                    xmlName: "ServerEncrypted",
                    serializedName: "ServerEncrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                incrementalCopy: {
                    xmlName: "IncrementalCopy",
                    serializedName: "IncrementalCopy",
                    type: {
                        name: "Boolean"
                    }
                },
                destinationSnapshot: {
                    xmlName: "DestinationSnapshot",
                    serializedName: "DestinationSnapshot",
                    type: {
                        name: "String"
                    }
                },
                deletedTime: {
                    xmlName: "DeletedTime",
                    serializedName: "DeletedTime",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                remainingRetentionDays: {
                    xmlName: "RemainingRetentionDays",
                    serializedName: "RemainingRetentionDays",
                    type: {
                        name: "Number"
                    }
                },
                accessTier: {
                    xmlName: "AccessTier",
                    serializedName: "AccessTier",
                    type: {
                        name: "String"
                    }
                },
                accessTierInferred: {
                    xmlName: "AccessTierInferred",
                    serializedName: "AccessTierInferred",
                    type: {
                        name: "Boolean"
                    }
                },
                archiveStatus: {
                    xmlName: "ArchiveStatus",
                    serializedName: "ArchiveStatus",
                    type: {
                        name: "String"
                    }
                },
                customerProvidedKeySha256: {
                    xmlName: "CustomerProvidedKeySha256",
                    serializedName: "CustomerProvidedKeySha256",
                    type: {
                        name: "String"
                    }
                },
                accessTierChangeTime: {
                    xmlName: "AccessTierChangeTime",
                    serializedName: "AccessTierChangeTime",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                }
            }
        }
    };
    var BlobMetadata = {
        xmlName: "Metadata",
        serializedName: "BlobMetadata",
        type: {
            name: "Composite",
            className: "BlobMetadata",
            modelProperties: {
                encrypted: {
                    xmlIsAttribute: true,
                    xmlName: "Encrypted",
                    serializedName: "Encrypted",
                    type: {
                        name: "String"
                    }
                }
            },
            additionalProperties: {
                type: {
                    name: "String"
                }
            }
        }
    };
    var BlobItem = {
        xmlName: "Blob",
        serializedName: "BlobItem",
        type: {
            name: "Composite",
            className: "BlobItem",
            modelProperties: {
                name: {
                    xmlName: "Name",
                    required: true,
                    serializedName: "Name",
                    type: {
                        name: "String"
                    }
                },
                deleted: {
                    xmlName: "Deleted",
                    required: true,
                    serializedName: "Deleted",
                    type: {
                        name: "Boolean"
                    }
                },
                snapshot: {
                    xmlName: "Snapshot",
                    required: true,
                    serializedName: "Snapshot",
                    type: {
                        name: "String"
                    }
                },
                properties: {
                    xmlName: "Properties",
                    required: true,
                    serializedName: "Properties",
                    type: {
                        name: "Composite",
                        className: "BlobProperties"
                    }
                },
                metadata: {
                    xmlName: "Metadata",
                    serializedName: "Metadata",
                    type: {
                        name: "Composite",
                        className: "BlobMetadata",
                        additionalProperties: {
                            type: {
                                name: "String"
                            }
                        }
                    }
                }
            }
        }
    };
    var BlobFlatListSegment = {
        xmlName: "Blobs",
        serializedName: "BlobFlatListSegment",
        type: {
            name: "Composite",
            className: "BlobFlatListSegment",
            modelProperties: {
                blobItems: {
                    xmlName: "BlobItems",
                    xmlElementName: "Blob",
                    required: true,
                    serializedName: "BlobItems",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "Composite",
                                className: "BlobItem"
                            }
                        }
                    }
                }
            }
        }
    };
    var ListBlobsFlatSegmentResponse = {
        xmlName: "EnumerationResults",
        serializedName: "ListBlobsFlatSegmentResponse",
        type: {
            name: "Composite",
            className: "ListBlobsFlatSegmentResponse",
            modelProperties: {
                serviceEndpoint: {
                    xmlIsAttribute: true,
                    xmlName: "ServiceEndpoint",
                    required: true,
                    serializedName: "ServiceEndpoint",
                    type: {
                        name: "String"
                    }
                },
                containerName: {
                    xmlIsAttribute: true,
                    xmlName: "ContainerName",
                    required: true,
                    serializedName: "ContainerName",
                    type: {
                        name: "String"
                    }
                },
                prefix: {
                    xmlName: "Prefix",
                    serializedName: "Prefix",
                    type: {
                        name: "String"
                    }
                },
                marker: {
                    xmlName: "Marker",
                    serializedName: "Marker",
                    type: {
                        name: "String"
                    }
                },
                maxResults: {
                    xmlName: "MaxResults",
                    serializedName: "MaxResults",
                    type: {
                        name: "Number"
                    }
                },
                delimiter: {
                    xmlName: "Delimiter",
                    serializedName: "Delimiter",
                    type: {
                        name: "String"
                    }
                },
                segment: {
                    xmlName: "Blobs",
                    required: true,
                    serializedName: "Segment",
                    type: {
                        name: "Composite",
                        className: "BlobFlatListSegment"
                    }
                },
                nextMarker: {
                    xmlName: "NextMarker",
                    serializedName: "NextMarker",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobPrefix = {
        serializedName: "BlobPrefix",
        type: {
            name: "Composite",
            className: "BlobPrefix",
            modelProperties: {
                name: {
                    xmlName: "Name",
                    required: true,
                    serializedName: "Name",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobHierarchyListSegment = {
        xmlName: "Blobs",
        serializedName: "BlobHierarchyListSegment",
        type: {
            name: "Composite",
            className: "BlobHierarchyListSegment",
            modelProperties: {
                blobPrefixes: {
                    xmlName: "BlobPrefixes",
                    xmlElementName: "BlobPrefix",
                    serializedName: "BlobPrefixes",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "Composite",
                                className: "BlobPrefix"
                            }
                        }
                    }
                },
                blobItems: {
                    xmlName: "BlobItems",
                    xmlElementName: "Blob",
                    required: true,
                    serializedName: "BlobItems",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "Composite",
                                className: "BlobItem"
                            }
                        }
                    }
                }
            }
        }
    };
    var ListBlobsHierarchySegmentResponse = {
        xmlName: "EnumerationResults",
        serializedName: "ListBlobsHierarchySegmentResponse",
        type: {
            name: "Composite",
            className: "ListBlobsHierarchySegmentResponse",
            modelProperties: {
                serviceEndpoint: {
                    xmlIsAttribute: true,
                    xmlName: "ServiceEndpoint",
                    required: true,
                    serializedName: "ServiceEndpoint",
                    type: {
                        name: "String"
                    }
                },
                containerName: {
                    xmlIsAttribute: true,
                    xmlName: "ContainerName",
                    required: true,
                    serializedName: "ContainerName",
                    type: {
                        name: "String"
                    }
                },
                prefix: {
                    xmlName: "Prefix",
                    serializedName: "Prefix",
                    type: {
                        name: "String"
                    }
                },
                marker: {
                    xmlName: "Marker",
                    serializedName: "Marker",
                    type: {
                        name: "String"
                    }
                },
                maxResults: {
                    xmlName: "MaxResults",
                    serializedName: "MaxResults",
                    type: {
                        name: "Number"
                    }
                },
                delimiter: {
                    xmlName: "Delimiter",
                    serializedName: "Delimiter",
                    type: {
                        name: "String"
                    }
                },
                segment: {
                    xmlName: "Blobs",
                    required: true,
                    serializedName: "Segment",
                    type: {
                        name: "Composite",
                        className: "BlobHierarchyListSegment"
                    }
                },
                nextMarker: {
                    xmlName: "NextMarker",
                    serializedName: "NextMarker",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var Block = {
        serializedName: "Block",
        type: {
            name: "Composite",
            className: "Block",
            modelProperties: {
                name: {
                    xmlName: "Name",
                    required: true,
                    serializedName: "Name",
                    type: {
                        name: "String"
                    }
                },
                size: {
                    xmlName: "Size",
                    required: true,
                    serializedName: "Size",
                    type: {
                        name: "Number"
                    }
                }
            }
        }
    };
    var BlockList = {
        serializedName: "BlockList",
        type: {
            name: "Composite",
            className: "BlockList",
            modelProperties: {
                committedBlocks: {
                    xmlIsWrapped: true,
                    xmlName: "CommittedBlocks",
                    xmlElementName: "Block",
                    serializedName: "CommittedBlocks",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "Composite",
                                className: "Block"
                            }
                        }
                    }
                },
                uncommittedBlocks: {
                    xmlIsWrapped: true,
                    xmlName: "UncommittedBlocks",
                    xmlElementName: "Block",
                    serializedName: "UncommittedBlocks",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "Composite",
                                className: "Block"
                            }
                        }
                    }
                }
            }
        }
    };
    var BlockLookupList = {
        xmlName: "BlockList",
        serializedName: "BlockLookupList",
        type: {
            name: "Composite",
            className: "BlockLookupList",
            modelProperties: {
                committed: {
                    xmlName: "Committed",
                    xmlElementName: "Committed",
                    serializedName: "Committed",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "String"
                            }
                        }
                    }
                },
                uncommitted: {
                    xmlName: "Uncommitted",
                    xmlElementName: "Uncommitted",
                    serializedName: "Uncommitted",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "String"
                            }
                        }
                    }
                },
                latest: {
                    xmlName: "Latest",
                    xmlElementName: "Latest",
                    serializedName: "Latest",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "String"
                            }
                        }
                    }
                }
            }
        }
    };
    var ContainerProperties = {
        serializedName: "ContainerProperties",
        type: {
            name: "Composite",
            className: "ContainerProperties",
            modelProperties: {
                lastModified: {
                    xmlName: "Last-Modified",
                    required: true,
                    serializedName: "Last-Modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                etag: {
                    xmlName: "Etag",
                    required: true,
                    serializedName: "Etag",
                    type: {
                        name: "String"
                    }
                },
                leaseStatus: {
                    xmlName: "LeaseStatus",
                    serializedName: "LeaseStatus",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "locked",
                            "unlocked"
                        ]
                    }
                },
                leaseState: {
                    xmlName: "LeaseState",
                    serializedName: "LeaseState",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "available",
                            "leased",
                            "expired",
                            "breaking",
                            "broken"
                        ]
                    }
                },
                leaseDuration: {
                    xmlName: "LeaseDuration",
                    serializedName: "LeaseDuration",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "infinite",
                            "fixed"
                        ]
                    }
                },
                publicAccess: {
                    xmlName: "PublicAccess",
                    serializedName: "PublicAccess",
                    type: {
                        name: "String"
                    }
                },
                hasImmutabilityPolicy: {
                    xmlName: "HasImmutabilityPolicy",
                    serializedName: "HasImmutabilityPolicy",
                    type: {
                        name: "Boolean"
                    }
                },
                hasLegalHold: {
                    xmlName: "HasLegalHold",
                    serializedName: "HasLegalHold",
                    type: {
                        name: "Boolean"
                    }
                }
            }
        }
    };
    var ContainerItem = {
        xmlName: "Container",
        serializedName: "ContainerItem",
        type: {
            name: "Composite",
            className: "ContainerItem",
            modelProperties: {
                name: {
                    xmlName: "Name",
                    required: true,
                    serializedName: "Name",
                    type: {
                        name: "String"
                    }
                },
                properties: {
                    xmlName: "Properties",
                    required: true,
                    serializedName: "Properties",
                    type: {
                        name: "Composite",
                        className: "ContainerProperties"
                    }
                },
                metadata: {
                    xmlName: "Metadata",
                    serializedName: "Metadata",
                    type: {
                        name: "Dictionary",
                        value: {
                            type: {
                                name: "String"
                            }
                        }
                    }
                }
            }
        }
    };
    var ListContainersSegmentResponse = {
        xmlName: "EnumerationResults",
        serializedName: "ListContainersSegmentResponse",
        type: {
            name: "Composite",
            className: "ListContainersSegmentResponse",
            modelProperties: {
                serviceEndpoint: {
                    xmlIsAttribute: true,
                    xmlName: "ServiceEndpoint",
                    required: true,
                    serializedName: "ServiceEndpoint",
                    type: {
                        name: "String"
                    }
                },
                prefix: {
                    xmlName: "Prefix",
                    serializedName: "Prefix",
                    type: {
                        name: "String"
                    }
                },
                marker: {
                    xmlName: "Marker",
                    serializedName: "Marker",
                    type: {
                        name: "String"
                    }
                },
                maxResults: {
                    xmlName: "MaxResults",
                    serializedName: "MaxResults",
                    type: {
                        name: "Number"
                    }
                },
                containerItems: {
                    xmlIsWrapped: true,
                    xmlName: "Containers",
                    xmlElementName: "Container",
                    required: true,
                    serializedName: "ContainerItems",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "Composite",
                                className: "ContainerItem"
                            }
                        }
                    }
                },
                nextMarker: {
                    xmlName: "NextMarker",
                    serializedName: "NextMarker",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var CorsRule = {
        serializedName: "CorsRule",
        type: {
            name: "Composite",
            className: "CorsRule",
            modelProperties: {
                allowedOrigins: {
                    xmlName: "AllowedOrigins",
                    required: true,
                    serializedName: "AllowedOrigins",
                    type: {
                        name: "String"
                    }
                },
                allowedMethods: {
                    xmlName: "AllowedMethods",
                    required: true,
                    serializedName: "AllowedMethods",
                    type: {
                        name: "String"
                    }
                },
                allowedHeaders: {
                    xmlName: "AllowedHeaders",
                    required: true,
                    serializedName: "AllowedHeaders",
                    type: {
                        name: "String"
                    }
                },
                exposedHeaders: {
                    xmlName: "ExposedHeaders",
                    required: true,
                    serializedName: "ExposedHeaders",
                    type: {
                        name: "String"
                    }
                },
                maxAgeInSeconds: {
                    xmlName: "MaxAgeInSeconds",
                    required: true,
                    serializedName: "MaxAgeInSeconds",
                    constraints: {
                        InclusiveMinimum: 0
                    },
                    type: {
                        name: "Number"
                    }
                }
            }
        }
    };
    var GeoReplication = {
        serializedName: "GeoReplication",
        type: {
            name: "Composite",
            className: "GeoReplication",
            modelProperties: {
                status: {
                    xmlName: "Status",
                    required: true,
                    serializedName: "Status",
                    type: {
                        name: "String"
                    }
                },
                lastSyncTime: {
                    xmlName: "LastSyncTime",
                    required: true,
                    serializedName: "LastSyncTime",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                }
            }
        }
    };
    var RetentionPolicy = {
        serializedName: "RetentionPolicy",
        type: {
            name: "Composite",
            className: "RetentionPolicy",
            modelProperties: {
                enabled: {
                    xmlName: "Enabled",
                    required: true,
                    serializedName: "Enabled",
                    type: {
                        name: "Boolean"
                    }
                },
                days: {
                    xmlName: "Days",
                    serializedName: "Days",
                    constraints: {
                        InclusiveMinimum: 1
                    },
                    type: {
                        name: "Number"
                    }
                }
            }
        }
    };
    var Logging = {
        serializedName: "Logging",
        type: {
            name: "Composite",
            className: "Logging",
            modelProperties: {
                version: {
                    xmlName: "Version",
                    required: true,
                    serializedName: "Version",
                    type: {
                        name: "String"
                    }
                },
                deleteProperty: {
                    xmlName: "Delete",
                    required: true,
                    serializedName: "Delete",
                    type: {
                        name: "Boolean"
                    }
                },
                read: {
                    xmlName: "Read",
                    required: true,
                    serializedName: "Read",
                    type: {
                        name: "Boolean"
                    }
                },
                write: {
                    xmlName: "Write",
                    required: true,
                    serializedName: "Write",
                    type: {
                        name: "Boolean"
                    }
                },
                retentionPolicy: {
                    xmlName: "RetentionPolicy",
                    required: true,
                    serializedName: "RetentionPolicy",
                    type: {
                        name: "Composite",
                        className: "RetentionPolicy"
                    }
                }
            }
        }
    };
    var Metrics = {
        serializedName: "Metrics",
        type: {
            name: "Composite",
            className: "Metrics",
            modelProperties: {
                version: {
                    xmlName: "Version",
                    serializedName: "Version",
                    type: {
                        name: "String"
                    }
                },
                enabled: {
                    xmlName: "Enabled",
                    required: true,
                    serializedName: "Enabled",
                    type: {
                        name: "Boolean"
                    }
                },
                includeAPIs: {
                    xmlName: "IncludeAPIs",
                    serializedName: "IncludeAPIs",
                    type: {
                        name: "Boolean"
                    }
                },
                retentionPolicy: {
                    xmlName: "RetentionPolicy",
                    serializedName: "RetentionPolicy",
                    type: {
                        name: "Composite",
                        className: "RetentionPolicy"
                    }
                }
            }
        }
    };
    var PageRange = {
        serializedName: "PageRange",
        type: {
            name: "Composite",
            className: "PageRange",
            modelProperties: {
                start: {
                    xmlName: "Start",
                    required: true,
                    serializedName: "Start",
                    type: {
                        name: "Number"
                    }
                },
                end: {
                    xmlName: "End",
                    required: true,
                    serializedName: "End",
                    type: {
                        name: "Number"
                    }
                }
            }
        }
    };
    var ClearRange = {
        serializedName: "ClearRange",
        type: {
            name: "Composite",
            className: "ClearRange",
            modelProperties: {
                start: {
                    xmlName: "Start",
                    required: true,
                    serializedName: "Start",
                    type: {
                        name: "Number"
                    }
                },
                end: {
                    xmlName: "End",
                    required: true,
                    serializedName: "End",
                    type: {
                        name: "Number"
                    }
                }
            }
        }
    };
    var PageList = {
        serializedName: "PageList",
        type: {
            name: "Composite",
            className: "PageList",
            modelProperties: {
                pageRange: {
                    xmlName: "PageRange",
                    xmlElementName: "PageRange",
                    serializedName: "PageRange",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "Composite",
                                className: "PageRange"
                            }
                        }
                    }
                },
                clearRange: {
                    xmlName: "ClearRange",
                    xmlElementName: "ClearRange",
                    serializedName: "ClearRange",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "Composite",
                                className: "ClearRange"
                            }
                        }
                    }
                }
            }
        }
    };
    var SignedIdentifier = {
        serializedName: "SignedIdentifier",
        type: {
            name: "Composite",
            className: "SignedIdentifier",
            modelProperties: {
                id: {
                    xmlName: "Id",
                    required: true,
                    serializedName: "Id",
                    type: {
                        name: "String"
                    }
                },
                accessPolicy: {
                    xmlName: "AccessPolicy",
                    required: true,
                    serializedName: "AccessPolicy",
                    type: {
                        name: "Composite",
                        className: "AccessPolicy"
                    }
                }
            }
        }
    };
    var StaticWebsite = {
        serializedName: "StaticWebsite",
        type: {
            name: "Composite",
            className: "StaticWebsite",
            modelProperties: {
                enabled: {
                    xmlName: "Enabled",
                    required: true,
                    serializedName: "Enabled",
                    type: {
                        name: "Boolean"
                    }
                },
                indexDocument: {
                    xmlName: "IndexDocument",
                    serializedName: "IndexDocument",
                    type: {
                        name: "String"
                    }
                },
                errorDocument404Path: {
                    xmlName: "ErrorDocument404Path",
                    serializedName: "ErrorDocument404Path",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var StorageServiceProperties = {
        serializedName: "StorageServiceProperties",
        type: {
            name: "Composite",
            className: "StorageServiceProperties",
            modelProperties: {
                logging: {
                    xmlName: "Logging",
                    serializedName: "Logging",
                    type: {
                        name: "Composite",
                        className: "Logging"
                    }
                },
                hourMetrics: {
                    xmlName: "HourMetrics",
                    serializedName: "HourMetrics",
                    type: {
                        name: "Composite",
                        className: "Metrics"
                    }
                },
                minuteMetrics: {
                    xmlName: "MinuteMetrics",
                    serializedName: "MinuteMetrics",
                    type: {
                        name: "Composite",
                        className: "Metrics"
                    }
                },
                cors: {
                    xmlIsWrapped: true,
                    xmlName: "Cors",
                    xmlElementName: "CorsRule",
                    serializedName: "Cors",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "Composite",
                                className: "CorsRule"
                            }
                        }
                    }
                },
                defaultServiceVersion: {
                    xmlName: "DefaultServiceVersion",
                    serializedName: "DefaultServiceVersion",
                    type: {
                        name: "String"
                    }
                },
                deleteRetentionPolicy: {
                    xmlName: "DeleteRetentionPolicy",
                    serializedName: "DeleteRetentionPolicy",
                    type: {
                        name: "Composite",
                        className: "RetentionPolicy"
                    }
                },
                staticWebsite: {
                    xmlName: "StaticWebsite",
                    serializedName: "StaticWebsite",
                    type: {
                        name: "Composite",
                        className: "StaticWebsite"
                    }
                }
            }
        }
    };
    var StorageServiceStats = {
        serializedName: "StorageServiceStats",
        type: {
            name: "Composite",
            className: "StorageServiceStats",
            modelProperties: {
                geoReplication: {
                    xmlName: "GeoReplication",
                    serializedName: "GeoReplication",
                    type: {
                        name: "Composite",
                        className: "GeoReplication"
                    }
                }
            }
        }
    };
    var ServiceSetPropertiesHeaders = {
        serializedName: "service-setproperties-headers",
        type: {
            name: "Composite",
            className: "ServiceSetPropertiesHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ServiceGetPropertiesHeaders = {
        serializedName: "service-getproperties-headers",
        type: {
            name: "Composite",
            className: "ServiceGetPropertiesHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ServiceGetStatisticsHeaders = {
        serializedName: "service-getstatistics-headers",
        type: {
            name: "Composite",
            className: "ServiceGetStatisticsHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ServiceListContainersSegmentHeaders = {
        serializedName: "service-listcontainerssegment-headers",
        type: {
            name: "Composite",
            className: "ServiceListContainersSegmentHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ServiceGetUserDelegationKeyHeaders = {
        serializedName: "service-getuserdelegationkey-headers",
        type: {
            name: "Composite",
            className: "ServiceGetUserDelegationKeyHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ServiceGetAccountInfoHeaders = {
        serializedName: "service-getaccountinfo-headers",
        type: {
            name: "Composite",
            className: "ServiceGetAccountInfoHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                skuName: {
                    serializedName: "x-ms-sku-name",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "Standard_LRS",
                            "Standard_GRS",
                            "Standard_RAGRS",
                            "Standard_ZRS",
                            "Premium_LRS"
                        ]
                    }
                },
                accountKind: {
                    serializedName: "x-ms-account-kind",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "Storage",
                            "BlobStorage",
                            "StorageV2"
                        ]
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ServiceSubmitBatchHeaders = {
        serializedName: "service-submitbatch-headers",
        type: {
            name: "Composite",
            className: "ServiceSubmitBatchHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                contentType: {
                    serializedName: "content-type",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerCreateHeaders = {
        serializedName: "container-create-headers",
        type: {
            name: "Composite",
            className: "ContainerCreateHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerGetPropertiesHeaders = {
        serializedName: "container-getproperties-headers",
        type: {
            name: "Composite",
            className: "ContainerGetPropertiesHeaders",
            modelProperties: {
                metadata: {
                    serializedName: "x-ms-meta",
                    type: {
                        name: "Dictionary",
                        value: {
                            type: {
                                name: "String"
                            }
                        }
                    },
                    headerCollectionPrefix: "x-ms-meta-"
                },
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                leaseDuration: {
                    serializedName: "x-ms-lease-duration",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "infinite",
                            "fixed"
                        ]
                    }
                },
                leaseState: {
                    serializedName: "x-ms-lease-state",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "available",
                            "leased",
                            "expired",
                            "breaking",
                            "broken"
                        ]
                    }
                },
                leaseStatus: {
                    serializedName: "x-ms-lease-status",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "locked",
                            "unlocked"
                        ]
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                blobPublicAccess: {
                    serializedName: "x-ms-blob-public-access",
                    type: {
                        name: "String"
                    }
                },
                hasImmutabilityPolicy: {
                    serializedName: "x-ms-has-immutability-policy",
                    type: {
                        name: "Boolean"
                    }
                },
                hasLegalHold: {
                    serializedName: "x-ms-has-legal-hold",
                    type: {
                        name: "Boolean"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerDeleteHeaders = {
        serializedName: "container-delete-headers",
        type: {
            name: "Composite",
            className: "ContainerDeleteHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerSetMetadataHeaders = {
        serializedName: "container-setmetadata-headers",
        type: {
            name: "Composite",
            className: "ContainerSetMetadataHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerGetAccessPolicyHeaders = {
        serializedName: "container-getaccesspolicy-headers",
        type: {
            name: "Composite",
            className: "ContainerGetAccessPolicyHeaders",
            modelProperties: {
                blobPublicAccess: {
                    serializedName: "x-ms-blob-public-access",
                    type: {
                        name: "String"
                    }
                },
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerSetAccessPolicyHeaders = {
        serializedName: "container-setaccesspolicy-headers",
        type: {
            name: "Composite",
            className: "ContainerSetAccessPolicyHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerAcquireLeaseHeaders = {
        serializedName: "container-acquirelease-headers",
        type: {
            name: "Composite",
            className: "ContainerAcquireLeaseHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                leaseId: {
                    serializedName: "x-ms-lease-id",
                    type: {
                        name: "String"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerReleaseLeaseHeaders = {
        serializedName: "container-releaselease-headers",
        type: {
            name: "Composite",
            className: "ContainerReleaseLeaseHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerRenewLeaseHeaders = {
        serializedName: "container-renewlease-headers",
        type: {
            name: "Composite",
            className: "ContainerRenewLeaseHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                leaseId: {
                    serializedName: "x-ms-lease-id",
                    type: {
                        name: "String"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerBreakLeaseHeaders = {
        serializedName: "container-breaklease-headers",
        type: {
            name: "Composite",
            className: "ContainerBreakLeaseHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                leaseTime: {
                    serializedName: "x-ms-lease-time",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerChangeLeaseHeaders = {
        serializedName: "container-changelease-headers",
        type: {
            name: "Composite",
            className: "ContainerChangeLeaseHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                leaseId: {
                    serializedName: "x-ms-lease-id",
                    type: {
                        name: "String"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerListBlobFlatSegmentHeaders = {
        serializedName: "container-listblobflatsegment-headers",
        type: {
            name: "Composite",
            className: "ContainerListBlobFlatSegmentHeaders",
            modelProperties: {
                contentType: {
                    serializedName: "content-type",
                    type: {
                        name: "String"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerListBlobHierarchySegmentHeaders = {
        serializedName: "container-listblobhierarchysegment-headers",
        type: {
            name: "Composite",
            className: "ContainerListBlobHierarchySegmentHeaders",
            modelProperties: {
                contentType: {
                    serializedName: "content-type",
                    type: {
                        name: "String"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var ContainerGetAccountInfoHeaders = {
        serializedName: "container-getaccountinfo-headers",
        type: {
            name: "Composite",
            className: "ContainerGetAccountInfoHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                skuName: {
                    serializedName: "x-ms-sku-name",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "Standard_LRS",
                            "Standard_GRS",
                            "Standard_RAGRS",
                            "Standard_ZRS",
                            "Premium_LRS"
                        ]
                    }
                },
                accountKind: {
                    serializedName: "x-ms-account-kind",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "Storage",
                            "BlobStorage",
                            "StorageV2"
                        ]
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobDownloadHeaders = {
        serializedName: "blob-download-headers",
        type: {
            name: "Composite",
            className: "BlobDownloadHeaders",
            modelProperties: {
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                metadata: {
                    serializedName: "x-ms-meta",
                    type: {
                        name: "Dictionary",
                        value: {
                            type: {
                                name: "String"
                            }
                        }
                    },
                    headerCollectionPrefix: "x-ms-meta-"
                },
                contentLength: {
                    serializedName: "content-length",
                    type: {
                        name: "Number"
                    }
                },
                contentType: {
                    serializedName: "content-type",
                    type: {
                        name: "String"
                    }
                },
                contentRange: {
                    serializedName: "content-range",
                    type: {
                        name: "String"
                    }
                },
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                contentEncoding: {
                    serializedName: "content-encoding",
                    type: {
                        name: "String"
                    }
                },
                cacheControl: {
                    serializedName: "cache-control",
                    type: {
                        name: "String"
                    }
                },
                contentDisposition: {
                    serializedName: "content-disposition",
                    type: {
                        name: "String"
                    }
                },
                contentLanguage: {
                    serializedName: "content-language",
                    type: {
                        name: "String"
                    }
                },
                blobSequenceNumber: {
                    serializedName: "x-ms-blob-sequence-number",
                    type: {
                        name: "Number"
                    }
                },
                blobType: {
                    serializedName: "x-ms-blob-type",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "BlockBlob",
                            "PageBlob",
                            "AppendBlob"
                        ]
                    }
                },
                copyCompletionTime: {
                    serializedName: "x-ms-copy-completion-time",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                copyStatusDescription: {
                    serializedName: "x-ms-copy-status-description",
                    type: {
                        name: "String"
                    }
                },
                copyId: {
                    serializedName: "x-ms-copy-id",
                    type: {
                        name: "String"
                    }
                },
                copyProgress: {
                    serializedName: "x-ms-copy-progress",
                    type: {
                        name: "String"
                    }
                },
                copySource: {
                    serializedName: "x-ms-copy-source",
                    type: {
                        name: "String"
                    }
                },
                copyStatus: {
                    serializedName: "x-ms-copy-status",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "pending",
                            "success",
                            "aborted",
                            "failed"
                        ]
                    }
                },
                leaseDuration: {
                    serializedName: "x-ms-lease-duration",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "infinite",
                            "fixed"
                        ]
                    }
                },
                leaseState: {
                    serializedName: "x-ms-lease-state",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "available",
                            "leased",
                            "expired",
                            "breaking",
                            "broken"
                        ]
                    }
                },
                leaseStatus: {
                    serializedName: "x-ms-lease-status",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "locked",
                            "unlocked"
                        ]
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                acceptRanges: {
                    serializedName: "accept-ranges",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                blobCommittedBlockCount: {
                    serializedName: "x-ms-blob-committed-block-count",
                    type: {
                        name: "Number"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                blobContentMD5: {
                    serializedName: "x-ms-blob-content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                contentCrc64: {
                    serializedName: "x-ms-content-crc64",
                    type: {
                        name: "ByteArray"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobGetPropertiesHeaders = {
        serializedName: "blob-getproperties-headers",
        type: {
            name: "Composite",
            className: "BlobGetPropertiesHeaders",
            modelProperties: {
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                creationTime: {
                    serializedName: "x-ms-creation-time",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                metadata: {
                    serializedName: "x-ms-meta",
                    type: {
                        name: "Dictionary",
                        value: {
                            type: {
                                name: "String"
                            }
                        }
                    },
                    headerCollectionPrefix: "x-ms-meta-"
                },
                blobType: {
                    serializedName: "x-ms-blob-type",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "BlockBlob",
                            "PageBlob",
                            "AppendBlob"
                        ]
                    }
                },
                copyCompletionTime: {
                    serializedName: "x-ms-copy-completion-time",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                copyStatusDescription: {
                    serializedName: "x-ms-copy-status-description",
                    type: {
                        name: "String"
                    }
                },
                copyId: {
                    serializedName: "x-ms-copy-id",
                    type: {
                        name: "String"
                    }
                },
                copyProgress: {
                    serializedName: "x-ms-copy-progress",
                    type: {
                        name: "String"
                    }
                },
                copySource: {
                    serializedName: "x-ms-copy-source",
                    type: {
                        name: "String"
                    }
                },
                copyStatus: {
                    serializedName: "x-ms-copy-status",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "pending",
                            "success",
                            "aborted",
                            "failed"
                        ]
                    }
                },
                isIncrementalCopy: {
                    serializedName: "x-ms-incremental-copy",
                    type: {
                        name: "Boolean"
                    }
                },
                destinationSnapshot: {
                    serializedName: "x-ms-copy-destination-snapshot",
                    type: {
                        name: "String"
                    }
                },
                leaseDuration: {
                    serializedName: "x-ms-lease-duration",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "infinite",
                            "fixed"
                        ]
                    }
                },
                leaseState: {
                    serializedName: "x-ms-lease-state",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "available",
                            "leased",
                            "expired",
                            "breaking",
                            "broken"
                        ]
                    }
                },
                leaseStatus: {
                    serializedName: "x-ms-lease-status",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "locked",
                            "unlocked"
                        ]
                    }
                },
                contentLength: {
                    serializedName: "content-length",
                    type: {
                        name: "Number"
                    }
                },
                contentType: {
                    serializedName: "content-type",
                    type: {
                        name: "String"
                    }
                },
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                contentEncoding: {
                    serializedName: "content-encoding",
                    type: {
                        name: "String"
                    }
                },
                contentDisposition: {
                    serializedName: "content-disposition",
                    type: {
                        name: "String"
                    }
                },
                contentLanguage: {
                    serializedName: "content-language",
                    type: {
                        name: "String"
                    }
                },
                cacheControl: {
                    serializedName: "cache-control",
                    type: {
                        name: "String"
                    }
                },
                blobSequenceNumber: {
                    serializedName: "x-ms-blob-sequence-number",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                acceptRanges: {
                    serializedName: "accept-ranges",
                    type: {
                        name: "String"
                    }
                },
                blobCommittedBlockCount: {
                    serializedName: "x-ms-blob-committed-block-count",
                    type: {
                        name: "Number"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                accessTier: {
                    serializedName: "x-ms-access-tier",
                    type: {
                        name: "String"
                    }
                },
                accessTierInferred: {
                    serializedName: "x-ms-access-tier-inferred",
                    type: {
                        name: "Boolean"
                    }
                },
                archiveStatus: {
                    serializedName: "x-ms-archive-status",
                    type: {
                        name: "String"
                    }
                },
                accessTierChangeTime: {
                    serializedName: "x-ms-access-tier-change-time",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobDeleteHeaders = {
        serializedName: "blob-delete-headers",
        type: {
            name: "Composite",
            className: "BlobDeleteHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobSetAccessControlHeaders = {
        serializedName: "blob-setaccesscontrol-headers",
        type: {
            name: "Composite",
            className: "BlobSetAccessControlHeaders",
            modelProperties: {
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobGetAccessControlHeaders = {
        serializedName: "blob-getaccesscontrol-headers",
        type: {
            name: "Composite",
            className: "BlobGetAccessControlHeaders",
            modelProperties: {
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                xMsOwner: {
                    serializedName: "x-ms-owner",
                    type: {
                        name: "String"
                    }
                },
                xMsGroup: {
                    serializedName: "x-ms-group",
                    type: {
                        name: "String"
                    }
                },
                xMsPermissions: {
                    serializedName: "x-ms-permissions",
                    type: {
                        name: "String"
                    }
                },
                xMsAcl: {
                    serializedName: "x-ms-acl",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobRenameHeaders = {
        serializedName: "blob-rename-headers",
        type: {
            name: "Composite",
            className: "BlobRenameHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                contentLength: {
                    serializedName: "content-length",
                    type: {
                        name: "Number"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                }
            }
        }
    };
    var PageBlobCreateHeaders = {
        serializedName: "pageblob-create-headers",
        type: {
            name: "Composite",
            className: "PageBlobCreateHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var AppendBlobCreateHeaders = {
        serializedName: "appendblob-create-headers",
        type: {
            name: "Composite",
            className: "AppendBlobCreateHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlockBlobUploadHeaders = {
        serializedName: "blockblob-upload-headers",
        type: {
            name: "Composite",
            className: "BlockBlobUploadHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobUndeleteHeaders = {
        serializedName: "blob-undelete-headers",
        type: {
            name: "Composite",
            className: "BlobUndeleteHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobSetHTTPHeadersHeaders = {
        serializedName: "blob-sethttpheaders-headers",
        type: {
            name: "Composite",
            className: "BlobSetHTTPHeadersHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                blobSequenceNumber: {
                    serializedName: "x-ms-blob-sequence-number",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobSetMetadataHeaders = {
        serializedName: "blob-setmetadata-headers",
        type: {
            name: "Composite",
            className: "BlobSetMetadataHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobAcquireLeaseHeaders = {
        serializedName: "blob-acquirelease-headers",
        type: {
            name: "Composite",
            className: "BlobAcquireLeaseHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                leaseId: {
                    serializedName: "x-ms-lease-id",
                    type: {
                        name: "String"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobReleaseLeaseHeaders = {
        serializedName: "blob-releaselease-headers",
        type: {
            name: "Composite",
            className: "BlobReleaseLeaseHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobRenewLeaseHeaders = {
        serializedName: "blob-renewlease-headers",
        type: {
            name: "Composite",
            className: "BlobRenewLeaseHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                leaseId: {
                    serializedName: "x-ms-lease-id",
                    type: {
                        name: "String"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobChangeLeaseHeaders = {
        serializedName: "blob-changelease-headers",
        type: {
            name: "Composite",
            className: "BlobChangeLeaseHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                leaseId: {
                    serializedName: "x-ms-lease-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobBreakLeaseHeaders = {
        serializedName: "blob-breaklease-headers",
        type: {
            name: "Composite",
            className: "BlobBreakLeaseHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                leaseTime: {
                    serializedName: "x-ms-lease-time",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobCreateSnapshotHeaders = {
        serializedName: "blob-createsnapshot-headers",
        type: {
            name: "Composite",
            className: "BlobCreateSnapshotHeaders",
            modelProperties: {
                snapshot: {
                    serializedName: "x-ms-snapshot",
                    type: {
                        name: "String"
                    }
                },
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobStartCopyFromURLHeaders = {
        serializedName: "blob-startcopyfromurl-headers",
        type: {
            name: "Composite",
            className: "BlobStartCopyFromURLHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                copyId: {
                    serializedName: "x-ms-copy-id",
                    type: {
                        name: "String"
                    }
                },
                copyStatus: {
                    serializedName: "x-ms-copy-status",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "pending",
                            "success",
                            "aborted",
                            "failed"
                        ]
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobCopyFromURLHeaders = {
        serializedName: "blob-copyfromurl-headers",
        type: {
            name: "Composite",
            className: "BlobCopyFromURLHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                copyId: {
                    serializedName: "x-ms-copy-id",
                    type: {
                        name: "String"
                    }
                },
                copyStatus: {
                    serializedName: "x-ms-copy-status",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "success"
                        ]
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobAbortCopyFromURLHeaders = {
        serializedName: "blob-abortcopyfromurl-headers",
        type: {
            name: "Composite",
            className: "BlobAbortCopyFromURLHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobSetTierHeaders = {
        serializedName: "blob-settier-headers",
        type: {
            name: "Composite",
            className: "BlobSetTierHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlobGetAccountInfoHeaders = {
        serializedName: "blob-getaccountinfo-headers",
        type: {
            name: "Composite",
            className: "BlobGetAccountInfoHeaders",
            modelProperties: {
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                skuName: {
                    serializedName: "x-ms-sku-name",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "Standard_LRS",
                            "Standard_GRS",
                            "Standard_RAGRS",
                            "Standard_ZRS",
                            "Premium_LRS"
                        ]
                    }
                },
                accountKind: {
                    serializedName: "x-ms-account-kind",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "Storage",
                            "BlobStorage",
                            "StorageV2"
                        ]
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlockBlobStageBlockHeaders = {
        serializedName: "blockblob-stageblock-headers",
        type: {
            name: "Composite",
            className: "BlockBlobStageBlockHeaders",
            modelProperties: {
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                xMsContentCrc64: {
                    serializedName: "x-ms-content-crc64",
                    type: {
                        name: "ByteArray"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlockBlobStageBlockFromURLHeaders = {
        serializedName: "blockblob-stageblockfromurl-headers",
        type: {
            name: "Composite",
            className: "BlockBlobStageBlockFromURLHeaders",
            modelProperties: {
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                xMsContentCrc64: {
                    serializedName: "x-ms-content-crc64",
                    type: {
                        name: "ByteArray"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlockBlobCommitBlockListHeaders = {
        serializedName: "blockblob-commitblocklist-headers",
        type: {
            name: "Composite",
            className: "BlockBlobCommitBlockListHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                xMsContentCrc64: {
                    serializedName: "x-ms-content-crc64",
                    type: {
                        name: "ByteArray"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var BlockBlobGetBlockListHeaders = {
        serializedName: "blockblob-getblocklist-headers",
        type: {
            name: "Composite",
            className: "BlockBlobGetBlockListHeaders",
            modelProperties: {
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                contentType: {
                    serializedName: "content-type",
                    type: {
                        name: "String"
                    }
                },
                blobContentLength: {
                    serializedName: "x-ms-blob-content-length",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var PageBlobUploadPagesHeaders = {
        serializedName: "pageblob-uploadpages-headers",
        type: {
            name: "Composite",
            className: "PageBlobUploadPagesHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                xMsContentCrc64: {
                    serializedName: "x-ms-content-crc64",
                    type: {
                        name: "ByteArray"
                    }
                },
                blobSequenceNumber: {
                    serializedName: "x-ms-blob-sequence-number",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var PageBlobClearPagesHeaders = {
        serializedName: "pageblob-clearpages-headers",
        type: {
            name: "Composite",
            className: "PageBlobClearPagesHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                xMsContentCrc64: {
                    serializedName: "x-ms-content-crc64",
                    type: {
                        name: "ByteArray"
                    }
                },
                blobSequenceNumber: {
                    serializedName: "x-ms-blob-sequence-number",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var PageBlobUploadPagesFromURLHeaders = {
        serializedName: "pageblob-uploadpagesfromurl-headers",
        type: {
            name: "Composite",
            className: "PageBlobUploadPagesFromURLHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                xMsContentCrc64: {
                    serializedName: "x-ms-content-crc64",
                    type: {
                        name: "ByteArray"
                    }
                },
                blobSequenceNumber: {
                    serializedName: "x-ms-blob-sequence-number",
                    type: {
                        name: "Number"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var PageBlobGetPageRangesHeaders = {
        serializedName: "pageblob-getpageranges-headers",
        type: {
            name: "Composite",
            className: "PageBlobGetPageRangesHeaders",
            modelProperties: {
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                blobContentLength: {
                    serializedName: "x-ms-blob-content-length",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var PageBlobGetPageRangesDiffHeaders = {
        serializedName: "pageblob-getpagerangesdiff-headers",
        type: {
            name: "Composite",
            className: "PageBlobGetPageRangesDiffHeaders",
            modelProperties: {
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                blobContentLength: {
                    serializedName: "x-ms-blob-content-length",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var PageBlobResizeHeaders = {
        serializedName: "pageblob-resize-headers",
        type: {
            name: "Composite",
            className: "PageBlobResizeHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                blobSequenceNumber: {
                    serializedName: "x-ms-blob-sequence-number",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var PageBlobUpdateSequenceNumberHeaders = {
        serializedName: "pageblob-updatesequencenumber-headers",
        type: {
            name: "Composite",
            className: "PageBlobUpdateSequenceNumberHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                blobSequenceNumber: {
                    serializedName: "x-ms-blob-sequence-number",
                    type: {
                        name: "Number"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var PageBlobCopyIncrementalHeaders = {
        serializedName: "pageblob-copyincremental-headers",
        type: {
            name: "Composite",
            className: "PageBlobCopyIncrementalHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                copyId: {
                    serializedName: "x-ms-copy-id",
                    type: {
                        name: "String"
                    }
                },
                copyStatus: {
                    serializedName: "x-ms-copy-status",
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "pending",
                            "success",
                            "aborted",
                            "failed"
                        ]
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var AppendBlobAppendBlockHeaders = {
        serializedName: "appendblob-appendblock-headers",
        type: {
            name: "Composite",
            className: "AppendBlobAppendBlockHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                xMsContentCrc64: {
                    serializedName: "x-ms-content-crc64",
                    type: {
                        name: "ByteArray"
                    }
                },
                clientRequestId: {
                    serializedName: "x-ms-client-request-id",
                    type: {
                        name: "String"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                blobAppendOffset: {
                    serializedName: "x-ms-blob-append-offset",
                    type: {
                        name: "String"
                    }
                },
                blobCommittedBlockCount: {
                    serializedName: "x-ms-blob-committed-block-count",
                    type: {
                        name: "Number"
                    }
                },
                isServerEncrypted: {
                    serializedName: "x-ms-request-server-encrypted",
                    type: {
                        name: "Boolean"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };
    var AppendBlobAppendBlockFromUrlHeaders = {
        serializedName: "appendblob-appendblockfromurl-headers",
        type: {
            name: "Composite",
            className: "AppendBlobAppendBlockFromUrlHeaders",
            modelProperties: {
                eTag: {
                    serializedName: "etag",
                    type: {
                        name: "String"
                    }
                },
                lastModified: {
                    serializedName: "last-modified",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                contentMD5: {
                    serializedName: "content-md5",
                    type: {
                        name: "ByteArray"
                    }
                },
                xMsContentCrc64: {
                    serializedName: "x-ms-content-crc64",
                    type: {
                        name: "ByteArray"
                    }
                },
                requestId: {
                    serializedName: "x-ms-request-id",
                    type: {
                        name: "String"
                    }
                },
                version: {
                    serializedName: "x-ms-version",
                    type: {
                        name: "String"
                    }
                },
                date: {
                    serializedName: "date",
                    type: {
                        name: "DateTimeRfc1123"
                    }
                },
                blobAppendOffset: {
                    serializedName: "x-ms-blob-append-offset",
                    type: {
                        name: "String"
                    }
                },
                blobCommittedBlockCount: {
                    serializedName: "x-ms-blob-committed-block-count",
                    type: {
                        name: "Number"
                    }
                },
                encryptionKeySha256: {
                    serializedName: "x-ms-encryption-key-sha256",
                    type: {
                        name: "String"
                    }
                },
                errorCode: {
                    serializedName: "x-ms-error-code",
                    type: {
                        name: "String"
                    }
                }
            }
        }
    };

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is regenerated.
     */

    var Mappers = /*#__PURE__*/Object.freeze({
        ContainerItem: ContainerItem,
        ContainerProperties: ContainerProperties,
        CorsRule: CorsRule,
        GeoReplication: GeoReplication,
        KeyInfo: KeyInfo,
        ListContainersSegmentResponse: ListContainersSegmentResponse,
        Logging: Logging,
        Metrics: Metrics,
        RetentionPolicy: RetentionPolicy,
        ServiceGetAccountInfoHeaders: ServiceGetAccountInfoHeaders,
        ServiceGetPropertiesHeaders: ServiceGetPropertiesHeaders,
        ServiceGetStatisticsHeaders: ServiceGetStatisticsHeaders,
        ServiceGetUserDelegationKeyHeaders: ServiceGetUserDelegationKeyHeaders,
        ServiceListContainersSegmentHeaders: ServiceListContainersSegmentHeaders,
        ServiceSetPropertiesHeaders: ServiceSetPropertiesHeaders,
        ServiceSubmitBatchHeaders: ServiceSubmitBatchHeaders,
        StaticWebsite: StaticWebsite,
        StorageError: StorageError,
        StorageServiceProperties: StorageServiceProperties,
        StorageServiceStats: StorageServiceStats,
        UserDelegationKey: UserDelegationKey
    });

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for
     * license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is
     * regenerated.
     */
    var access = {
        parameterPath: [
            "options",
            "access"
        ],
        mapper: {
            serializedName: "x-ms-blob-public-access",
            type: {
                name: "String"
            }
        }
    };
    var action0 = {
        parameterPath: "action",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-lease-action",
            defaultValue: 'acquire',
            type: {
                name: "String"
            }
        }
    };
    var action1 = {
        parameterPath: "action",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-lease-action",
            defaultValue: 'release',
            type: {
                name: "String"
            }
        }
    };
    var action2 = {
        parameterPath: "action",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-lease-action",
            defaultValue: 'renew',
            type: {
                name: "String"
            }
        }
    };
    var action3 = {
        parameterPath: "action",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-lease-action",
            defaultValue: 'break',
            type: {
                name: "String"
            }
        }
    };
    var action4 = {
        parameterPath: "action",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-lease-action",
            defaultValue: 'change',
            type: {
                name: "String"
            }
        }
    };
    var action5 = {
        parameterPath: "action",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "action",
            defaultValue: 'setAccessControl',
            type: {
                name: "String"
            }
        }
    };
    var action6 = {
        parameterPath: "action",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "action",
            defaultValue: 'getAccessControl',
            type: {
                name: "String"
            }
        }
    };
    var appendPosition = {
        parameterPath: [
            "options",
            "appendPositionAccessConditions",
            "appendPosition"
        ],
        mapper: {
            serializedName: "x-ms-blob-condition-appendpos",
            type: {
                name: "Number"
            }
        }
    };
    var blobCacheControl = {
        parameterPath: [
            "options",
            "blobHTTPHeaders",
            "blobCacheControl"
        ],
        mapper: {
            serializedName: "x-ms-blob-cache-control",
            type: {
                name: "String"
            }
        }
    };
    var blobContentDisposition = {
        parameterPath: [
            "options",
            "blobHTTPHeaders",
            "blobContentDisposition"
        ],
        mapper: {
            serializedName: "x-ms-blob-content-disposition",
            type: {
                name: "String"
            }
        }
    };
    var blobContentEncoding = {
        parameterPath: [
            "options",
            "blobHTTPHeaders",
            "blobContentEncoding"
        ],
        mapper: {
            serializedName: "x-ms-blob-content-encoding",
            type: {
                name: "String"
            }
        }
    };
    var blobContentLanguage = {
        parameterPath: [
            "options",
            "blobHTTPHeaders",
            "blobContentLanguage"
        ],
        mapper: {
            serializedName: "x-ms-blob-content-language",
            type: {
                name: "String"
            }
        }
    };
    var blobContentLength = {
        parameterPath: "blobContentLength",
        mapper: {
            required: true,
            serializedName: "x-ms-blob-content-length",
            type: {
                name: "Number"
            }
        }
    };
    var blobContentMD5 = {
        parameterPath: [
            "options",
            "blobHTTPHeaders",
            "blobContentMD5"
        ],
        mapper: {
            serializedName: "x-ms-blob-content-md5",
            type: {
                name: "ByteArray"
            }
        }
    };
    var blobContentType = {
        parameterPath: [
            "options",
            "blobHTTPHeaders",
            "blobContentType"
        ],
        mapper: {
            serializedName: "x-ms-blob-content-type",
            type: {
                name: "String"
            }
        }
    };
    var blobSequenceNumber = {
        parameterPath: [
            "options",
            "blobSequenceNumber"
        ],
        mapper: {
            serializedName: "x-ms-blob-sequence-number",
            defaultValue: 0,
            type: {
                name: "Number"
            }
        }
    };
    var blobType0 = {
        parameterPath: "blobType",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-blob-type",
            defaultValue: 'PageBlob',
            type: {
                name: "String"
            }
        }
    };
    var blobType1 = {
        parameterPath: "blobType",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-blob-type",
            defaultValue: 'AppendBlob',
            type: {
                name: "String"
            }
        }
    };
    var blobType2 = {
        parameterPath: "blobType",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-blob-type",
            defaultValue: 'BlockBlob',
            type: {
                name: "String"
            }
        }
    };
    var blockId = {
        parameterPath: "blockId",
        mapper: {
            required: true,
            serializedName: "blockid",
            type: {
                name: "String"
            }
        }
    };
    var breakPeriod = {
        parameterPath: [
            "options",
            "breakPeriod"
        ],
        mapper: {
            serializedName: "x-ms-lease-break-period",
            type: {
                name: "Number"
            }
        }
    };
    var cacheControl = {
        parameterPath: [
            "options",
            "directoryHttpHeaders",
            "cacheControl"
        ],
        mapper: {
            serializedName: "x-ms-cache-control",
            type: {
                name: "String"
            }
        }
    };
    var comp0 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'properties',
            type: {
                name: "String"
            }
        }
    };
    var comp1 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'stats',
            type: {
                name: "String"
            }
        }
    };
    var comp10 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'copy',
            type: {
                name: "String"
            }
        }
    };
    var comp11 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'tier',
            type: {
                name: "String"
            }
        }
    };
    var comp12 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'page',
            type: {
                name: "String"
            }
        }
    };
    var comp13 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'pagelist',
            type: {
                name: "String"
            }
        }
    };
    var comp14 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'incrementalcopy',
            type: {
                name: "String"
            }
        }
    };
    var comp15 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'appendblock',
            type: {
                name: "String"
            }
        }
    };
    var comp16 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'block',
            type: {
                name: "String"
            }
        }
    };
    var comp17 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'blocklist',
            type: {
                name: "String"
            }
        }
    };
    var comp2 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'list',
            type: {
                name: "String"
            }
        }
    };
    var comp3 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'userdelegationkey',
            type: {
                name: "String"
            }
        }
    };
    var comp4 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'batch',
            type: {
                name: "String"
            }
        }
    };
    var comp5 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'metadata',
            type: {
                name: "String"
            }
        }
    };
    var comp6 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'acl',
            type: {
                name: "String"
            }
        }
    };
    var comp7 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'lease',
            type: {
                name: "String"
            }
        }
    };
    var comp8 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'undelete',
            type: {
                name: "String"
            }
        }
    };
    var comp9 = {
        parameterPath: "comp",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "comp",
            defaultValue: 'snapshot',
            type: {
                name: "String"
            }
        }
    };
    var contentDisposition = {
        parameterPath: [
            "options",
            "directoryHttpHeaders",
            "contentDisposition"
        ],
        mapper: {
            serializedName: "x-ms-content-disposition",
            type: {
                name: "String"
            }
        }
    };
    var contentEncoding = {
        parameterPath: [
            "options",
            "directoryHttpHeaders",
            "contentEncoding"
        ],
        mapper: {
            serializedName: "x-ms-content-encoding",
            type: {
                name: "String"
            }
        }
    };
    var contentLanguage = {
        parameterPath: [
            "options",
            "directoryHttpHeaders",
            "contentLanguage"
        ],
        mapper: {
            serializedName: "x-ms-content-language",
            type: {
                name: "String"
            }
        }
    };
    var contentLength = {
        parameterPath: "contentLength",
        mapper: {
            required: true,
            serializedName: "Content-Length",
            type: {
                name: "Number"
            }
        }
    };
    var contentType = {
        parameterPath: [
            "options",
            "directoryHttpHeaders",
            "contentType"
        ],
        mapper: {
            serializedName: "x-ms-content-type",
            type: {
                name: "String"
            }
        }
    };
    var copyActionAbortConstant = {
        parameterPath: "copyActionAbortConstant",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-copy-action",
            defaultValue: 'abort',
            type: {
                name: "String"
            }
        }
    };
    var copyId = {
        parameterPath: "copyId",
        mapper: {
            required: true,
            serializedName: "copyid",
            type: {
                name: "String"
            }
        }
    };
    var copySource = {
        parameterPath: "copySource",
        mapper: {
            required: true,
            serializedName: "x-ms-copy-source",
            type: {
                name: "String"
            }
        }
    };
    var deleteSnapshots = {
        parameterPath: [
            "options",
            "deleteSnapshots"
        ],
        mapper: {
            serializedName: "x-ms-delete-snapshots",
            type: {
                name: "Enum",
                allowedValues: [
                    "include",
                    "only"
                ]
            }
        }
    };
    var delimiter = {
        parameterPath: "delimiter",
        mapper: {
            required: true,
            serializedName: "delimiter",
            type: {
                name: "String"
            }
        }
    };
    var directoryProperties = {
        parameterPath: [
            "options",
            "directoryProperties"
        ],
        mapper: {
            serializedName: "x-ms-properties",
            type: {
                name: "String"
            }
        }
    };
    var duration = {
        parameterPath: [
            "options",
            "duration"
        ],
        mapper: {
            serializedName: "x-ms-lease-duration",
            type: {
                name: "Number"
            }
        }
    };
    var encryptionAlgorithm = {
        parameterPath: [
            "options",
            "cpkInfo",
            "encryptionAlgorithm"
        ],
        mapper: {
            serializedName: "x-ms-encryption-algorithm",
            type: {
                name: "Enum",
                allowedValues: [
                    "AES256"
                ]
            }
        }
    };
    var encryptionKey = {
        parameterPath: [
            "options",
            "cpkInfo",
            "encryptionKey"
        ],
        mapper: {
            serializedName: "x-ms-encryption-key",
            type: {
                name: "String"
            }
        }
    };
    var encryptionKeySha256 = {
        parameterPath: [
            "options",
            "cpkInfo",
            "encryptionKeySha256"
        ],
        mapper: {
            serializedName: "x-ms-encryption-key-sha256",
            type: {
                name: "String"
            }
        }
    };
    var group = {
        parameterPath: [
            "options",
            "group"
        ],
        mapper: {
            serializedName: "x-ms-group",
            type: {
                name: "String"
            }
        }
    };
    var ifMatch = {
        parameterPath: [
            "options",
            "modifiedAccessConditions",
            "ifMatch"
        ],
        mapper: {
            serializedName: "If-Match",
            type: {
                name: "String"
            }
        }
    };
    var ifModifiedSince = {
        parameterPath: [
            "options",
            "modifiedAccessConditions",
            "ifModifiedSince"
        ],
        mapper: {
            serializedName: "If-Modified-Since",
            type: {
                name: "DateTimeRfc1123"
            }
        }
    };
    var ifNoneMatch = {
        parameterPath: [
            "options",
            "modifiedAccessConditions",
            "ifNoneMatch"
        ],
        mapper: {
            serializedName: "If-None-Match",
            type: {
                name: "String"
            }
        }
    };
    var ifSequenceNumberEqualTo = {
        parameterPath: [
            "options",
            "sequenceNumberAccessConditions",
            "ifSequenceNumberEqualTo"
        ],
        mapper: {
            serializedName: "x-ms-if-sequence-number-eq",
            type: {
                name: "Number"
            }
        }
    };
    var ifSequenceNumberLessThan = {
        parameterPath: [
            "options",
            "sequenceNumberAccessConditions",
            "ifSequenceNumberLessThan"
        ],
        mapper: {
            serializedName: "x-ms-if-sequence-number-lt",
            type: {
                name: "Number"
            }
        }
    };
    var ifSequenceNumberLessThanOrEqualTo = {
        parameterPath: [
            "options",
            "sequenceNumberAccessConditions",
            "ifSequenceNumberLessThanOrEqualTo"
        ],
        mapper: {
            serializedName: "x-ms-if-sequence-number-le",
            type: {
                name: "Number"
            }
        }
    };
    var ifUnmodifiedSince = {
        parameterPath: [
            "options",
            "modifiedAccessConditions",
            "ifUnmodifiedSince"
        ],
        mapper: {
            serializedName: "If-Unmodified-Since",
            type: {
                name: "DateTimeRfc1123"
            }
        }
    };
    var include0 = {
        parameterPath: [
            "options",
            "include"
        ],
        mapper: {
            serializedName: "include",
            type: {
                name: "Enum",
                allowedValues: [
                    "metadata"
                ]
            }
        }
    };
    var include1 = {
        parameterPath: [
            "options",
            "include"
        ],
        mapper: {
            serializedName: "include",
            type: {
                name: "Sequence",
                element: {
                    type: {
                        name: "Enum",
                        allowedValues: [
                            "copy",
                            "deleted",
                            "metadata",
                            "snapshots",
                            "uncommittedblobs"
                        ]
                    }
                }
            }
        },
        collectionFormat: QueryCollectionFormat.Csv
    };
    var leaseId0 = {
        parameterPath: [
            "options",
            "leaseAccessConditions",
            "leaseId"
        ],
        mapper: {
            serializedName: "x-ms-lease-id",
            type: {
                name: "String"
            }
        }
    };
    var leaseId1 = {
        parameterPath: "leaseId",
        mapper: {
            required: true,
            serializedName: "x-ms-lease-id",
            type: {
                name: "String"
            }
        }
    };
    var listType = {
        parameterPath: "listType",
        mapper: {
            required: true,
            serializedName: "blocklisttype",
            defaultValue: 'committed',
            type: {
                name: "Enum",
                allowedValues: [
                    "committed",
                    "uncommitted",
                    "all"
                ]
            }
        }
    };
    var marker0 = {
        parameterPath: [
            "options",
            "marker"
        ],
        mapper: {
            serializedName: "marker",
            type: {
                name: "String"
            }
        }
    };
    var maxresults = {
        parameterPath: [
            "options",
            "maxresults"
        ],
        mapper: {
            serializedName: "maxresults",
            constraints: {
                InclusiveMinimum: 1
            },
            type: {
                name: "Number"
            }
        }
    };
    var maxSize = {
        parameterPath: [
            "options",
            "appendPositionAccessConditions",
            "maxSize"
        ],
        mapper: {
            serializedName: "x-ms-blob-condition-maxsize",
            type: {
                name: "Number"
            }
        }
    };
    var metadata = {
        parameterPath: [
            "options",
            "metadata"
        ],
        mapper: {
            serializedName: "x-ms-meta",
            type: {
                name: "Dictionary",
                value: {
                    type: {
                        name: "String"
                    }
                }
            },
            headerCollectionPrefix: "x-ms-meta-"
        }
    };
    var multipartContentType = {
        parameterPath: "multipartContentType",
        mapper: {
            required: true,
            serializedName: "Content-Type",
            type: {
                name: "String"
            }
        }
    };
    var owner = {
        parameterPath: [
            "options",
            "owner"
        ],
        mapper: {
            serializedName: "x-ms-owner",
            type: {
                name: "String"
            }
        }
    };
    var pageWrite0 = {
        parameterPath: "pageWrite",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-page-write",
            defaultValue: 'update',
            type: {
                name: "String"
            }
        }
    };
    var pageWrite1 = {
        parameterPath: "pageWrite",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-page-write",
            defaultValue: 'clear',
            type: {
                name: "String"
            }
        }
    };
    var pathRenameMode = {
        parameterPath: "pathRenameMode",
        mapper: {
            serializedName: "mode",
            type: {
                name: "Enum",
                allowedValues: [
                    "legacy",
                    "posix"
                ]
            }
        }
    };
    var posixAcl = {
        parameterPath: [
            "options",
            "posixAcl"
        ],
        mapper: {
            serializedName: "x-ms-acl",
            type: {
                name: "String"
            }
        }
    };
    var posixPermissions = {
        parameterPath: [
            "options",
            "posixPermissions"
        ],
        mapper: {
            serializedName: "x-ms-permissions",
            type: {
                name: "String"
            }
        }
    };
    var posixUmask = {
        parameterPath: [
            "options",
            "posixUmask"
        ],
        mapper: {
            serializedName: "x-ms-umask",
            type: {
                name: "String"
            }
        }
    };
    var prefix = {
        parameterPath: [
            "options",
            "prefix"
        ],
        mapper: {
            serializedName: "prefix",
            type: {
                name: "String"
            }
        }
    };
    var prevsnapshot = {
        parameterPath: [
            "options",
            "prevsnapshot"
        ],
        mapper: {
            serializedName: "prevsnapshot",
            type: {
                name: "String"
            }
        }
    };
    var proposedLeaseId0 = {
        parameterPath: [
            "options",
            "proposedLeaseId"
        ],
        mapper: {
            serializedName: "x-ms-proposed-lease-id",
            type: {
                name: "String"
            }
        }
    };
    var proposedLeaseId1 = {
        parameterPath: "proposedLeaseId",
        mapper: {
            required: true,
            serializedName: "x-ms-proposed-lease-id",
            type: {
                name: "String"
            }
        }
    };
    var range0 = {
        parameterPath: [
            "options",
            "range"
        ],
        mapper: {
            serializedName: "x-ms-range",
            type: {
                name: "String"
            }
        }
    };
    var range1 = {
        parameterPath: "range",
        mapper: {
            required: true,
            serializedName: "x-ms-range",
            type: {
                name: "String"
            }
        }
    };
    var rangeGetContentCRC64 = {
        parameterPath: [
            "options",
            "rangeGetContentCRC64"
        ],
        mapper: {
            serializedName: "x-ms-range-get-content-crc64",
            type: {
                name: "Boolean"
            }
        }
    };
    var rangeGetContentMD5 = {
        parameterPath: [
            "options",
            "rangeGetContentMD5"
        ],
        mapper: {
            serializedName: "x-ms-range-get-content-md5",
            type: {
                name: "Boolean"
            }
        }
    };
    var rehydratePriority = {
        parameterPath: [
            "options",
            "rehydratePriority"
        ],
        mapper: {
            serializedName: "x-ms-rehydrate-priority",
            type: {
                name: "String"
            }
        }
    };
    var renameSource = {
        parameterPath: "renameSource",
        mapper: {
            required: true,
            serializedName: "x-ms-rename-source",
            type: {
                name: "String"
            }
        }
    };
    var requestId = {
        parameterPath: [
            "options",
            "requestId"
        ],
        mapper: {
            serializedName: "x-ms-client-request-id",
            type: {
                name: "String"
            }
        }
    };
    var restype0 = {
        parameterPath: "restype",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "restype",
            defaultValue: 'service',
            type: {
                name: "String"
            }
        }
    };
    var restype1 = {
        parameterPath: "restype",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "restype",
            defaultValue: 'account',
            type: {
                name: "String"
            }
        }
    };
    var restype2 = {
        parameterPath: "restype",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "restype",
            defaultValue: 'container',
            type: {
                name: "String"
            }
        }
    };
    var sequenceNumberAction = {
        parameterPath: "sequenceNumberAction",
        mapper: {
            required: true,
            serializedName: "x-ms-sequence-number-action",
            type: {
                name: "Enum",
                allowedValues: [
                    "max",
                    "update",
                    "increment"
                ]
            }
        }
    };
    var snapshot = {
        parameterPath: [
            "options",
            "snapshot"
        ],
        mapper: {
            serializedName: "snapshot",
            type: {
                name: "String"
            }
        }
    };
    var sourceContentCrc64 = {
        parameterPath: [
            "options",
            "sourceContentCrc64"
        ],
        mapper: {
            serializedName: "x-ms-source-content-crc64",
            type: {
                name: "ByteArray"
            }
        }
    };
    var sourceContentMD5 = {
        parameterPath: [
            "options",
            "sourceContentMD5"
        ],
        mapper: {
            serializedName: "x-ms-source-content-md5",
            type: {
                name: "ByteArray"
            }
        }
    };
    var sourceIfMatch = {
        parameterPath: [
            "options",
            "sourceModifiedAccessConditions",
            "sourceIfMatch"
        ],
        mapper: {
            serializedName: "x-ms-source-if-match",
            type: {
                name: "String"
            }
        }
    };
    var sourceIfModifiedSince = {
        parameterPath: [
            "options",
            "sourceModifiedAccessConditions",
            "sourceIfModifiedSince"
        ],
        mapper: {
            serializedName: "x-ms-source-if-modified-since",
            type: {
                name: "DateTimeRfc1123"
            }
        }
    };
    var sourceIfNoneMatch = {
        parameterPath: [
            "options",
            "sourceModifiedAccessConditions",
            "sourceIfNoneMatch"
        ],
        mapper: {
            serializedName: "x-ms-source-if-none-match",
            type: {
                name: "String"
            }
        }
    };
    var sourceIfUnmodifiedSince = {
        parameterPath: [
            "options",
            "sourceModifiedAccessConditions",
            "sourceIfUnmodifiedSince"
        ],
        mapper: {
            serializedName: "x-ms-source-if-unmodified-since",
            type: {
                name: "DateTimeRfc1123"
            }
        }
    };
    var sourceLeaseId = {
        parameterPath: [
            "options",
            "sourceLeaseId"
        ],
        mapper: {
            serializedName: "x-ms-source-lease-id",
            type: {
                name: "String"
            }
        }
    };
    var sourceRange0 = {
        parameterPath: "sourceRange",
        mapper: {
            required: true,
            serializedName: "x-ms-source-range",
            type: {
                name: "String"
            }
        }
    };
    var sourceRange1 = {
        parameterPath: [
            "options",
            "sourceRange"
        ],
        mapper: {
            serializedName: "x-ms-source-range",
            type: {
                name: "String"
            }
        }
    };
    var sourceUrl = {
        parameterPath: "sourceUrl",
        mapper: {
            required: true,
            serializedName: "x-ms-copy-source",
            type: {
                name: "String"
            }
        }
    };
    var tier0 = {
        parameterPath: [
            "options",
            "tier"
        ],
        mapper: {
            serializedName: "x-ms-access-tier",
            type: {
                name: "String"
            }
        }
    };
    var tier1 = {
        parameterPath: "tier",
        mapper: {
            required: true,
            serializedName: "x-ms-access-tier",
            type: {
                name: "String"
            }
        }
    };
    var timeout = {
        parameterPath: [
            "options",
            "timeout"
        ],
        mapper: {
            serializedName: "timeout",
            constraints: {
                InclusiveMinimum: 0
            },
            type: {
                name: "Number"
            }
        }
    };
    var transactionalContentCrc64 = {
        parameterPath: [
            "options",
            "transactionalContentCrc64"
        ],
        mapper: {
            serializedName: "x-ms-content-crc64",
            type: {
                name: "ByteArray"
            }
        }
    };
    var transactionalContentMD5 = {
        parameterPath: [
            "options",
            "transactionalContentMD5"
        ],
        mapper: {
            serializedName: "Content-MD5",
            type: {
                name: "ByteArray"
            }
        }
    };
    var upn = {
        parameterPath: [
            "options",
            "upn"
        ],
        mapper: {
            serializedName: "upn",
            type: {
                name: "Boolean"
            }
        }
    };
    var url = {
        parameterPath: "url",
        mapper: {
            required: true,
            serializedName: "url",
            defaultValue: '',
            type: {
                name: "String"
            }
        },
        skipEncoding: true
    };
    var version = {
        parameterPath: "version",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-version",
            defaultValue: '2019-02-02',
            type: {
                name: "String"
            }
        }
    };
    var xMsRequiresSync = {
        parameterPath: "xMsRequiresSync",
        mapper: {
            required: true,
            isConstant: true,
            serializedName: "x-ms-requires-sync",
            defaultValue: 'true',
            type: {
                name: "String"
            }
        }
    };

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for
     * license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is
     * regenerated.
     */
    /** Class representing a Service. */
    var Service = /** @class */ (function () {
        /**
         * Create a Service.
         * @param {StorageClientContext} client Reference to the service client.
         */
        function Service(client) {
            this.client = client;
        }
        Service.prototype.setProperties = function (storageServiceProperties, options, callback) {
            return this.client.sendOperationRequest({
                storageServiceProperties: storageServiceProperties,
                options: options
            }, setPropertiesOperationSpec, callback);
        };
        Service.prototype.getProperties = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getPropertiesOperationSpec, callback);
        };
        Service.prototype.getStatistics = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getStatisticsOperationSpec, callback);
        };
        Service.prototype.listContainersSegment = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, listContainersSegmentOperationSpec, callback);
        };
        Service.prototype.getUserDelegationKey = function (keyInfo, options, callback) {
            return this.client.sendOperationRequest({
                keyInfo: keyInfo,
                options: options
            }, getUserDelegationKeyOperationSpec, callback);
        };
        Service.prototype.getAccountInfo = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getAccountInfoOperationSpec, callback);
        };
        Service.prototype.submitBatch = function (body, contentLength, multipartContentType, options, callback) {
            return this.client.sendOperationRequest({
                body: body,
                contentLength: contentLength,
                multipartContentType: multipartContentType,
                options: options
            }, submitBatchOperationSpec, callback);
        };
        return Service;
    }());
    // Operation Specifications
    var serializer$1 = new Serializer(Mappers, true);
    var setPropertiesOperationSpec = {
        httpMethod: "PUT",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            restype0,
            comp0
        ],
        headerParameters: [
            version,
            requestId
        ],
        requestBody: {
            parameterPath: "storageServiceProperties",
            mapper: __assign({}, StorageServiceProperties, { required: true })
        },
        contentType: "application/xml; charset=utf-8",
        responses: {
            202: {
                headersMapper: ServiceSetPropertiesHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$1
    };
    var getPropertiesOperationSpec = {
        httpMethod: "GET",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            restype0,
            comp0
        ],
        headerParameters: [
            version,
            requestId
        ],
        responses: {
            200: {
                bodyMapper: StorageServiceProperties,
                headersMapper: ServiceGetPropertiesHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$1
    };
    var getStatisticsOperationSpec = {
        httpMethod: "GET",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            restype0,
            comp1
        ],
        headerParameters: [
            version,
            requestId
        ],
        responses: {
            200: {
                bodyMapper: StorageServiceStats,
                headersMapper: ServiceGetStatisticsHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$1
    };
    var listContainersSegmentOperationSpec = {
        httpMethod: "GET",
        urlParameters: [
            url
        ],
        queryParameters: [
            prefix,
            marker0,
            maxresults,
            include0,
            timeout,
            comp2
        ],
        headerParameters: [
            version,
            requestId
        ],
        responses: {
            200: {
                bodyMapper: ListContainersSegmentResponse,
                headersMapper: ServiceListContainersSegmentHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$1
    };
    var getUserDelegationKeyOperationSpec = {
        httpMethod: "POST",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            restype0,
            comp3
        ],
        headerParameters: [
            version,
            requestId
        ],
        requestBody: {
            parameterPath: "keyInfo",
            mapper: __assign({}, KeyInfo, { required: true })
        },
        contentType: "application/xml; charset=utf-8",
        responses: {
            200: {
                bodyMapper: UserDelegationKey,
                headersMapper: ServiceGetUserDelegationKeyHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$1
    };
    var getAccountInfoOperationSpec = {
        httpMethod: "GET",
        urlParameters: [
            url
        ],
        queryParameters: [
            restype1,
            comp0
        ],
        headerParameters: [
            version
        ],
        responses: {
            200: {
                headersMapper: ServiceGetAccountInfoHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$1
    };
    var submitBatchOperationSpec = {
        httpMethod: "POST",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp4
        ],
        headerParameters: [
            contentLength,
            multipartContentType,
            version,
            requestId
        ],
        requestBody: {
            parameterPath: "body",
            mapper: {
                required: true,
                serializedName: "body",
                type: {
                    name: "Stream"
                }
            }
        },
        contentType: "application/xml; charset=utf-8",
        responses: {
            202: {
                bodyMapper: {
                    serializedName: "parsedResponse",
                    type: {
                        name: "Stream"
                    }
                },
                headersMapper: ServiceSubmitBatchHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$1
    };

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is regenerated.
     */

    var Mappers$1 = /*#__PURE__*/Object.freeze({
        AccessPolicy: AccessPolicy,
        BlobFlatListSegment: BlobFlatListSegment,
        BlobHierarchyListSegment: BlobHierarchyListSegment,
        BlobItem: BlobItem,
        BlobMetadata: BlobMetadata,
        BlobPrefix: BlobPrefix,
        BlobProperties: BlobProperties,
        ContainerAcquireLeaseHeaders: ContainerAcquireLeaseHeaders,
        ContainerBreakLeaseHeaders: ContainerBreakLeaseHeaders,
        ContainerChangeLeaseHeaders: ContainerChangeLeaseHeaders,
        ContainerCreateHeaders: ContainerCreateHeaders,
        ContainerDeleteHeaders: ContainerDeleteHeaders,
        ContainerGetAccessPolicyHeaders: ContainerGetAccessPolicyHeaders,
        ContainerGetAccountInfoHeaders: ContainerGetAccountInfoHeaders,
        ContainerGetPropertiesHeaders: ContainerGetPropertiesHeaders,
        ContainerListBlobFlatSegmentHeaders: ContainerListBlobFlatSegmentHeaders,
        ContainerListBlobHierarchySegmentHeaders: ContainerListBlobHierarchySegmentHeaders,
        ContainerReleaseLeaseHeaders: ContainerReleaseLeaseHeaders,
        ContainerRenewLeaseHeaders: ContainerRenewLeaseHeaders,
        ContainerSetAccessPolicyHeaders: ContainerSetAccessPolicyHeaders,
        ContainerSetMetadataHeaders: ContainerSetMetadataHeaders,
        ListBlobsFlatSegmentResponse: ListBlobsFlatSegmentResponse,
        ListBlobsHierarchySegmentResponse: ListBlobsHierarchySegmentResponse,
        SignedIdentifier: SignedIdentifier,
        StorageError: StorageError
    });

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for
     * license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is
     * regenerated.
     */
    /** Class representing a Container. */
    var Container = /** @class */ (function () {
        /**
         * Create a Container.
         * @param {StorageClientContext} client Reference to the service client.
         */
        function Container(client) {
            this.client = client;
        }
        Container.prototype.create = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, createOperationSpec, callback);
        };
        Container.prototype.getProperties = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getPropertiesOperationSpec$1, callback);
        };
        Container.prototype.deleteMethod = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, deleteMethodOperationSpec, callback);
        };
        Container.prototype.setMetadata = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, setMetadataOperationSpec, callback);
        };
        Container.prototype.getAccessPolicy = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getAccessPolicyOperationSpec, callback);
        };
        Container.prototype.setAccessPolicy = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, setAccessPolicyOperationSpec, callback);
        };
        Container.prototype.acquireLease = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, acquireLeaseOperationSpec, callback);
        };
        Container.prototype.releaseLease = function (leaseId, options, callback) {
            return this.client.sendOperationRequest({
                leaseId: leaseId,
                options: options
            }, releaseLeaseOperationSpec, callback);
        };
        Container.prototype.renewLease = function (leaseId, options, callback) {
            return this.client.sendOperationRequest({
                leaseId: leaseId,
                options: options
            }, renewLeaseOperationSpec, callback);
        };
        Container.prototype.breakLease = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, breakLeaseOperationSpec, callback);
        };
        Container.prototype.changeLease = function (leaseId, proposedLeaseId, options, callback) {
            return this.client.sendOperationRequest({
                leaseId: leaseId,
                proposedLeaseId: proposedLeaseId,
                options: options
            }, changeLeaseOperationSpec, callback);
        };
        Container.prototype.listBlobFlatSegment = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, listBlobFlatSegmentOperationSpec, callback);
        };
        Container.prototype.listBlobHierarchySegment = function (delimiter, options, callback) {
            return this.client.sendOperationRequest({
                delimiter: delimiter,
                options: options
            }, listBlobHierarchySegmentOperationSpec, callback);
        };
        Container.prototype.getAccountInfo = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getAccountInfoOperationSpec$1, callback);
        };
        return Container;
    }());
    // Operation Specifications
    var serializer$2 = new Serializer(Mappers$1, true);
    var createOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            restype2
        ],
        headerParameters: [
            metadata,
            access,
            version,
            requestId
        ],
        responses: {
            201: {
                headersMapper: ContainerCreateHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var getPropertiesOperationSpec$1 = {
        httpMethod: "GET",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            restype2
        ],
        headerParameters: [
            version,
            requestId,
            leaseId0
        ],
        responses: {
            200: {
                headersMapper: ContainerGetPropertiesHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var deleteMethodOperationSpec = {
        httpMethod: "DELETE",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            restype2
        ],
        headerParameters: [
            version,
            requestId,
            leaseId0,
            ifModifiedSince,
            ifUnmodifiedSince
        ],
        responses: {
            202: {
                headersMapper: ContainerDeleteHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var setMetadataOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            restype2,
            comp5
        ],
        headerParameters: [
            metadata,
            version,
            requestId,
            leaseId0,
            ifModifiedSince
        ],
        responses: {
            200: {
                headersMapper: ContainerSetMetadataHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var getAccessPolicyOperationSpec = {
        httpMethod: "GET",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            restype2,
            comp6
        ],
        headerParameters: [
            version,
            requestId,
            leaseId0
        ],
        responses: {
            200: {
                bodyMapper: {
                    xmlElementName: "SignedIdentifier",
                    serializedName: "parsedResponse",
                    type: {
                        name: "Sequence",
                        element: {
                            type: {
                                name: "Composite",
                                className: "SignedIdentifier"
                            }
                        }
                    }
                },
                headersMapper: ContainerGetAccessPolicyHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var setAccessPolicyOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            restype2,
            comp6
        ],
        headerParameters: [
            access,
            version,
            requestId,
            leaseId0,
            ifModifiedSince,
            ifUnmodifiedSince
        ],
        requestBody: {
            parameterPath: [
                "options",
                "containerAcl"
            ],
            mapper: {
                xmlName: "SignedIdentifiers",
                xmlElementName: "SignedIdentifier",
                serializedName: "containerAcl",
                type: {
                    name: "Sequence",
                    element: {
                        type: {
                            name: "Composite",
                            className: "SignedIdentifier"
                        }
                    }
                }
            }
        },
        contentType: "application/xml; charset=utf-8",
        responses: {
            200: {
                headersMapper: ContainerSetAccessPolicyHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var acquireLeaseOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp7,
            restype2
        ],
        headerParameters: [
            duration,
            proposedLeaseId0,
            version,
            requestId,
            action0,
            ifModifiedSince,
            ifUnmodifiedSince
        ],
        responses: {
            201: {
                headersMapper: ContainerAcquireLeaseHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var releaseLeaseOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp7,
            restype2
        ],
        headerParameters: [
            leaseId1,
            version,
            requestId,
            action1,
            ifModifiedSince,
            ifUnmodifiedSince
        ],
        responses: {
            200: {
                headersMapper: ContainerReleaseLeaseHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var renewLeaseOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp7,
            restype2
        ],
        headerParameters: [
            leaseId1,
            version,
            requestId,
            action2,
            ifModifiedSince,
            ifUnmodifiedSince
        ],
        responses: {
            200: {
                headersMapper: ContainerRenewLeaseHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var breakLeaseOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp7,
            restype2
        ],
        headerParameters: [
            breakPeriod,
            version,
            requestId,
            action3,
            ifModifiedSince,
            ifUnmodifiedSince
        ],
        responses: {
            202: {
                headersMapper: ContainerBreakLeaseHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var changeLeaseOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp7,
            restype2
        ],
        headerParameters: [
            leaseId1,
            proposedLeaseId1,
            version,
            requestId,
            action4,
            ifModifiedSince,
            ifUnmodifiedSince
        ],
        responses: {
            200: {
                headersMapper: ContainerChangeLeaseHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var listBlobFlatSegmentOperationSpec = {
        httpMethod: "GET",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            prefix,
            marker0,
            maxresults,
            include1,
            timeout,
            restype2,
            comp2
        ],
        headerParameters: [
            version,
            requestId
        ],
        responses: {
            200: {
                bodyMapper: ListBlobsFlatSegmentResponse,
                headersMapper: ContainerListBlobFlatSegmentHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var listBlobHierarchySegmentOperationSpec = {
        httpMethod: "GET",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            prefix,
            delimiter,
            marker0,
            maxresults,
            include1,
            timeout,
            restype2,
            comp2
        ],
        headerParameters: [
            version,
            requestId
        ],
        responses: {
            200: {
                bodyMapper: ListBlobsHierarchySegmentResponse,
                headersMapper: ContainerListBlobHierarchySegmentHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };
    var getAccountInfoOperationSpec$1 = {
        httpMethod: "GET",
        path: "{containerName}",
        urlParameters: [
            url
        ],
        queryParameters: [
            restype1,
            comp0
        ],
        headerParameters: [
            version
        ],
        responses: {
            200: {
                headersMapper: ContainerGetAccountInfoHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$2
    };

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is regenerated.
     */

    var Mappers$2 = /*#__PURE__*/Object.freeze({
        BlobAbortCopyFromURLHeaders: BlobAbortCopyFromURLHeaders,
        BlobAcquireLeaseHeaders: BlobAcquireLeaseHeaders,
        BlobBreakLeaseHeaders: BlobBreakLeaseHeaders,
        BlobChangeLeaseHeaders: BlobChangeLeaseHeaders,
        BlobCopyFromURLHeaders: BlobCopyFromURLHeaders,
        BlobCreateSnapshotHeaders: BlobCreateSnapshotHeaders,
        BlobDeleteHeaders: BlobDeleteHeaders,
        BlobDownloadHeaders: BlobDownloadHeaders,
        BlobGetAccessControlHeaders: BlobGetAccessControlHeaders,
        BlobGetAccountInfoHeaders: BlobGetAccountInfoHeaders,
        BlobGetPropertiesHeaders: BlobGetPropertiesHeaders,
        BlobReleaseLeaseHeaders: BlobReleaseLeaseHeaders,
        BlobRenameHeaders: BlobRenameHeaders,
        BlobRenewLeaseHeaders: BlobRenewLeaseHeaders,
        BlobSetAccessControlHeaders: BlobSetAccessControlHeaders,
        BlobSetHTTPHeadersHeaders: BlobSetHTTPHeadersHeaders,
        BlobSetMetadataHeaders: BlobSetMetadataHeaders,
        BlobSetTierHeaders: BlobSetTierHeaders,
        BlobStartCopyFromURLHeaders: BlobStartCopyFromURLHeaders,
        BlobUndeleteHeaders: BlobUndeleteHeaders,
        DataLakeStorageError: DataLakeStorageError,
        DataLakeStorageErrorError: DataLakeStorageErrorError,
        StorageError: StorageError
    });

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for
     * license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is
     * regenerated.
     */
    /** Class representing a Blob. */
    var Blob$1 = /** @class */ (function () {
        /**
         * Create a Blob.
         * @param {StorageClientContext} client Reference to the service client.
         */
        function Blob(client) {
            this.client = client;
        }
        Blob.prototype.download = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, downloadOperationSpec, callback);
        };
        Blob.prototype.getProperties = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getPropertiesOperationSpec$2, callback);
        };
        Blob.prototype.deleteMethod = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, deleteMethodOperationSpec$1, callback);
        };
        Blob.prototype.setAccessControl = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, setAccessControlOperationSpec, callback);
        };
        Blob.prototype.getAccessControl = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getAccessControlOperationSpec, callback);
        };
        Blob.prototype.rename = function (renameSource, options, callback) {
            return this.client.sendOperationRequest({
                renameSource: renameSource,
                options: options
            }, renameOperationSpec, callback);
        };
        Blob.prototype.undelete = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, undeleteOperationSpec, callback);
        };
        Blob.prototype.setHTTPHeaders = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, setHTTPHeadersOperationSpec, callback);
        };
        Blob.prototype.setMetadata = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, setMetadataOperationSpec$1, callback);
        };
        Blob.prototype.acquireLease = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, acquireLeaseOperationSpec$1, callback);
        };
        Blob.prototype.releaseLease = function (leaseId, options, callback) {
            return this.client.sendOperationRequest({
                leaseId: leaseId,
                options: options
            }, releaseLeaseOperationSpec$1, callback);
        };
        Blob.prototype.renewLease = function (leaseId, options, callback) {
            return this.client.sendOperationRequest({
                leaseId: leaseId,
                options: options
            }, renewLeaseOperationSpec$1, callback);
        };
        Blob.prototype.changeLease = function (leaseId, proposedLeaseId, options, callback) {
            return this.client.sendOperationRequest({
                leaseId: leaseId,
                proposedLeaseId: proposedLeaseId,
                options: options
            }, changeLeaseOperationSpec$1, callback);
        };
        Blob.prototype.breakLease = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, breakLeaseOperationSpec$1, callback);
        };
        Blob.prototype.createSnapshot = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, createSnapshotOperationSpec, callback);
        };
        Blob.prototype.startCopyFromURL = function (copySource, options, callback) {
            return this.client.sendOperationRequest({
                copySource: copySource,
                options: options
            }, startCopyFromURLOperationSpec, callback);
        };
        Blob.prototype.copyFromURL = function (copySource, options, callback) {
            return this.client.sendOperationRequest({
                copySource: copySource,
                options: options
            }, copyFromURLOperationSpec, callback);
        };
        Blob.prototype.abortCopyFromURL = function (copyId, options, callback) {
            return this.client.sendOperationRequest({
                copyId: copyId,
                options: options
            }, abortCopyFromURLOperationSpec, callback);
        };
        Blob.prototype.setTier = function (tier, options, callback) {
            return this.client.sendOperationRequest({
                tier: tier,
                options: options
            }, setTierOperationSpec, callback);
        };
        Blob.prototype.getAccountInfo = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getAccountInfoOperationSpec$2, callback);
        };
        return Blob;
    }());
    // Operation Specifications
    var serializer$3 = new Serializer(Mappers$2, true);
    var downloadOperationSpec = {
        httpMethod: "GET",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            snapshot,
            timeout
        ],
        headerParameters: [
            range0,
            rangeGetContentMD5,
            rangeGetContentCRC64,
            version,
            requestId,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                bodyMapper: {
                    serializedName: "parsedResponse",
                    type: {
                        name: "Stream"
                    }
                },
                headersMapper: BlobDownloadHeaders
            },
            206: {
                bodyMapper: {
                    serializedName: "parsedResponse",
                    type: {
                        name: "Stream"
                    }
                },
                headersMapper: BlobDownloadHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var getPropertiesOperationSpec$2 = {
        httpMethod: "HEAD",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            snapshot,
            timeout
        ],
        headerParameters: [
            version,
            requestId,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                headersMapper: BlobGetPropertiesHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var deleteMethodOperationSpec$1 = {
        httpMethod: "DELETE",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            snapshot,
            timeout
        ],
        headerParameters: [
            deleteSnapshots,
            version,
            requestId,
            leaseId0,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            202: {
                headersMapper: BlobDeleteHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var setAccessControlOperationSpec = {
        httpMethod: "PATCH",
        path: "{filesystem}/{path}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            action5
        ],
        headerParameters: [
            owner,
            group,
            posixPermissions,
            posixAcl,
            requestId,
            version,
            leaseId0,
            ifMatch,
            ifNoneMatch,
            ifModifiedSince,
            ifUnmodifiedSince
        ],
        responses: {
            200: {
                headersMapper: BlobSetAccessControlHeaders
            },
            default: {
                bodyMapper: DataLakeStorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var getAccessControlOperationSpec = {
        httpMethod: "HEAD",
        path: "{filesystem}/{path}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            upn,
            action6
        ],
        headerParameters: [
            requestId,
            version,
            leaseId0,
            ifMatch,
            ifNoneMatch,
            ifModifiedSince,
            ifUnmodifiedSince
        ],
        responses: {
            200: {
                headersMapper: BlobGetAccessControlHeaders
            },
            default: {
                bodyMapper: DataLakeStorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var renameOperationSpec = {
        httpMethod: "PUT",
        path: "{filesystem}/{path}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            pathRenameMode
        ],
        headerParameters: [
            renameSource,
            directoryProperties,
            posixPermissions,
            posixUmask,
            sourceLeaseId,
            version,
            requestId,
            cacheControl,
            contentType,
            contentEncoding,
            contentLanguage,
            contentDisposition,
            leaseId0,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch,
            sourceIfModifiedSince,
            sourceIfUnmodifiedSince,
            sourceIfMatch,
            sourceIfNoneMatch
        ],
        responses: {
            201: {
                headersMapper: BlobRenameHeaders
            },
            default: {
                bodyMapper: DataLakeStorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var undeleteOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp8
        ],
        headerParameters: [
            version,
            requestId
        ],
        responses: {
            200: {
                headersMapper: BlobUndeleteHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var setHTTPHeadersOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp0
        ],
        headerParameters: [
            version,
            requestId,
            blobCacheControl,
            blobContentType,
            blobContentMD5,
            blobContentEncoding,
            blobContentLanguage,
            blobContentDisposition,
            leaseId0,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                headersMapper: BlobSetHTTPHeadersHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var setMetadataOperationSpec$1 = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp5
        ],
        headerParameters: [
            metadata,
            version,
            requestId,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                headersMapper: BlobSetMetadataHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var acquireLeaseOperationSpec$1 = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp7
        ],
        headerParameters: [
            duration,
            proposedLeaseId0,
            version,
            requestId,
            action0,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            201: {
                headersMapper: BlobAcquireLeaseHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var releaseLeaseOperationSpec$1 = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp7
        ],
        headerParameters: [
            leaseId1,
            version,
            requestId,
            action1,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                headersMapper: BlobReleaseLeaseHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var renewLeaseOperationSpec$1 = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp7
        ],
        headerParameters: [
            leaseId1,
            version,
            requestId,
            action2,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                headersMapper: BlobRenewLeaseHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var changeLeaseOperationSpec$1 = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp7
        ],
        headerParameters: [
            leaseId1,
            proposedLeaseId1,
            version,
            requestId,
            action4,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                headersMapper: BlobChangeLeaseHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var breakLeaseOperationSpec$1 = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp7
        ],
        headerParameters: [
            breakPeriod,
            version,
            requestId,
            action3,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            202: {
                headersMapper: BlobBreakLeaseHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var createSnapshotOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp9
        ],
        headerParameters: [
            metadata,
            version,
            requestId,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch,
            leaseId0
        ],
        responses: {
            201: {
                headersMapper: BlobCreateSnapshotHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var startCopyFromURLOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout
        ],
        headerParameters: [
            metadata,
            tier0,
            rehydratePriority,
            copySource,
            version,
            requestId,
            sourceIfModifiedSince,
            sourceIfUnmodifiedSince,
            sourceIfMatch,
            sourceIfNoneMatch,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch,
            leaseId0
        ],
        responses: {
            202: {
                headersMapper: BlobStartCopyFromURLHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var copyFromURLOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout
        ],
        headerParameters: [
            metadata,
            tier0,
            copySource,
            version,
            requestId,
            xMsRequiresSync,
            sourceIfModifiedSince,
            sourceIfUnmodifiedSince,
            sourceIfMatch,
            sourceIfNoneMatch,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch,
            leaseId0
        ],
        responses: {
            202: {
                headersMapper: BlobCopyFromURLHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var abortCopyFromURLOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            copyId,
            timeout,
            comp10
        ],
        headerParameters: [
            version,
            requestId,
            copyActionAbortConstant,
            leaseId0
        ],
        responses: {
            204: {
                headersMapper: BlobAbortCopyFromURLHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var setTierOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp11
        ],
        headerParameters: [
            tier1,
            rehydratePriority,
            version,
            requestId,
            leaseId0
        ],
        responses: {
            200: {
                headersMapper: BlobSetTierHeaders
            },
            202: {
                headersMapper: BlobSetTierHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };
    var getAccountInfoOperationSpec$2 = {
        httpMethod: "GET",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            restype1,
            comp0
        ],
        headerParameters: [
            version
        ],
        responses: {
            200: {
                headersMapper: BlobGetAccountInfoHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$3
    };

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is regenerated.
     */

    var Mappers$3 = /*#__PURE__*/Object.freeze({
        ClearRange: ClearRange,
        PageBlobClearPagesHeaders: PageBlobClearPagesHeaders,
        PageBlobCopyIncrementalHeaders: PageBlobCopyIncrementalHeaders,
        PageBlobCreateHeaders: PageBlobCreateHeaders,
        PageBlobGetPageRangesDiffHeaders: PageBlobGetPageRangesDiffHeaders,
        PageBlobGetPageRangesHeaders: PageBlobGetPageRangesHeaders,
        PageBlobResizeHeaders: PageBlobResizeHeaders,
        PageBlobUpdateSequenceNumberHeaders: PageBlobUpdateSequenceNumberHeaders,
        PageBlobUploadPagesFromURLHeaders: PageBlobUploadPagesFromURLHeaders,
        PageBlobUploadPagesHeaders: PageBlobUploadPagesHeaders,
        PageList: PageList,
        PageRange: PageRange,
        StorageError: StorageError
    });

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for
     * license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is
     * regenerated.
     */
    /** Class representing a PageBlob. */
    var PageBlob = /** @class */ (function () {
        /**
         * Create a PageBlob.
         * @param {StorageClientContext} client Reference to the service client.
         */
        function PageBlob(client) {
            this.client = client;
        }
        PageBlob.prototype.create = function (contentLength, blobContentLength, options, callback) {
            return this.client.sendOperationRequest({
                contentLength: contentLength,
                blobContentLength: blobContentLength,
                options: options
            }, createOperationSpec$1, callback);
        };
        PageBlob.prototype.uploadPages = function (body, contentLength, options, callback) {
            return this.client.sendOperationRequest({
                body: body,
                contentLength: contentLength,
                options: options
            }, uploadPagesOperationSpec, callback);
        };
        PageBlob.prototype.clearPages = function (contentLength, options, callback) {
            return this.client.sendOperationRequest({
                contentLength: contentLength,
                options: options
            }, clearPagesOperationSpec, callback);
        };
        PageBlob.prototype.uploadPagesFromURL = function (sourceUrl, sourceRange, contentLength, range, options, callback) {
            return this.client.sendOperationRequest({
                sourceUrl: sourceUrl,
                sourceRange: sourceRange,
                contentLength: contentLength,
                range: range,
                options: options
            }, uploadPagesFromURLOperationSpec, callback);
        };
        PageBlob.prototype.getPageRanges = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getPageRangesOperationSpec, callback);
        };
        PageBlob.prototype.getPageRangesDiff = function (options, callback) {
            return this.client.sendOperationRequest({
                options: options
            }, getPageRangesDiffOperationSpec, callback);
        };
        PageBlob.prototype.resize = function (blobContentLength, options, callback) {
            return this.client.sendOperationRequest({
                blobContentLength: blobContentLength,
                options: options
            }, resizeOperationSpec, callback);
        };
        PageBlob.prototype.updateSequenceNumber = function (sequenceNumberAction, options, callback) {
            return this.client.sendOperationRequest({
                sequenceNumberAction: sequenceNumberAction,
                options: options
            }, updateSequenceNumberOperationSpec, callback);
        };
        PageBlob.prototype.copyIncremental = function (copySource, options, callback) {
            return this.client.sendOperationRequest({
                copySource: copySource,
                options: options
            }, copyIncrementalOperationSpec, callback);
        };
        return PageBlob;
    }());
    // Operation Specifications
    var serializer$4 = new Serializer(Mappers$3, true);
    var createOperationSpec$1 = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout
        ],
        headerParameters: [
            contentLength,
            metadata,
            blobContentLength,
            blobSequenceNumber,
            version,
            requestId,
            tier0,
            blobType0,
            blobContentType,
            blobContentEncoding,
            blobContentLanguage,
            blobContentMD5,
            blobCacheControl,
            blobContentDisposition,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            201: {
                headersMapper: PageBlobCreateHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$4
    };
    var uploadPagesOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp12
        ],
        headerParameters: [
            contentLength,
            transactionalContentMD5,
            transactionalContentCrc64,
            range0,
            version,
            requestId,
            pageWrite0,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifSequenceNumberLessThanOrEqualTo,
            ifSequenceNumberLessThan,
            ifSequenceNumberEqualTo,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        requestBody: {
            parameterPath: "body",
            mapper: {
                required: true,
                serializedName: "body",
                type: {
                    name: "Stream"
                }
            }
        },
        contentType: "application/octet-stream",
        responses: {
            201: {
                headersMapper: PageBlobUploadPagesHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$4
    };
    var clearPagesOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp12
        ],
        headerParameters: [
            contentLength,
            range0,
            version,
            requestId,
            pageWrite1,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifSequenceNumberLessThanOrEqualTo,
            ifSequenceNumberLessThan,
            ifSequenceNumberEqualTo,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            201: {
                headersMapper: PageBlobClearPagesHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$4
    };
    var uploadPagesFromURLOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp12
        ],
        headerParameters: [
            sourceUrl,
            sourceRange0,
            sourceContentMD5,
            sourceContentCrc64,
            contentLength,
            range1,
            version,
            requestId,
            pageWrite0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            leaseId0,
            ifSequenceNumberLessThanOrEqualTo,
            ifSequenceNumberLessThan,
            ifSequenceNumberEqualTo,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch,
            sourceIfModifiedSince,
            sourceIfUnmodifiedSince,
            sourceIfMatch,
            sourceIfNoneMatch
        ],
        responses: {
            201: {
                headersMapper: PageBlobUploadPagesFromURLHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$4
    };
    var getPageRangesOperationSpec = {
        httpMethod: "GET",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            snapshot,
            timeout,
            comp13
        ],
        headerParameters: [
            range0,
            version,
            requestId,
            leaseId0,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                bodyMapper: PageList,
                headersMapper: PageBlobGetPageRangesHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$4
    };
    var getPageRangesDiffOperationSpec = {
        httpMethod: "GET",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            snapshot,
            timeout,
            prevsnapshot,
            comp13
        ],
        headerParameters: [
            range0,
            version,
            requestId,
            leaseId0,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                bodyMapper: PageList,
                headersMapper: PageBlobGetPageRangesDiffHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$4
    };
    var resizeOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp0
        ],
        headerParameters: [
            blobContentLength,
            version,
            requestId,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                headersMapper: PageBlobResizeHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$4
    };
    var updateSequenceNumberOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp0
        ],
        headerParameters: [
            sequenceNumberAction,
            blobSequenceNumber,
            version,
            requestId,
            leaseId0,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            200: {
                headersMapper: PageBlobUpdateSequenceNumberHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$4
    };
    var copyIncrementalOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp14
        ],
        headerParameters: [
            copySource,
            version,
            requestId,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            202: {
                headersMapper: PageBlobCopyIncrementalHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$4
    };

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is regenerated.
     */

    var Mappers$4 = /*#__PURE__*/Object.freeze({
        AppendBlobAppendBlockFromUrlHeaders: AppendBlobAppendBlockFromUrlHeaders,
        AppendBlobAppendBlockHeaders: AppendBlobAppendBlockHeaders,
        AppendBlobCreateHeaders: AppendBlobCreateHeaders,
        StorageError: StorageError
    });

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for
     * license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is
     * regenerated.
     */
    /** Class representing a AppendBlob. */
    var AppendBlob = /** @class */ (function () {
        /**
         * Create a AppendBlob.
         * @param {StorageClientContext} client Reference to the service client.
         */
        function AppendBlob(client) {
            this.client = client;
        }
        AppendBlob.prototype.create = function (contentLength, options, callback) {
            return this.client.sendOperationRequest({
                contentLength: contentLength,
                options: options
            }, createOperationSpec$2, callback);
        };
        AppendBlob.prototype.appendBlock = function (body, contentLength, options, callback) {
            return this.client.sendOperationRequest({
                body: body,
                contentLength: contentLength,
                options: options
            }, appendBlockOperationSpec, callback);
        };
        AppendBlob.prototype.appendBlockFromUrl = function (sourceUrl, contentLength, options, callback) {
            return this.client.sendOperationRequest({
                sourceUrl: sourceUrl,
                contentLength: contentLength,
                options: options
            }, appendBlockFromUrlOperationSpec, callback);
        };
        return AppendBlob;
    }());
    // Operation Specifications
    var serializer$5 = new Serializer(Mappers$4, true);
    var createOperationSpec$2 = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout
        ],
        headerParameters: [
            contentLength,
            metadata,
            version,
            requestId,
            blobType1,
            blobContentType,
            blobContentEncoding,
            blobContentLanguage,
            blobContentMD5,
            blobCacheControl,
            blobContentDisposition,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        responses: {
            201: {
                headersMapper: AppendBlobCreateHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$5
    };
    var appendBlockOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp15
        ],
        headerParameters: [
            contentLength,
            transactionalContentMD5,
            transactionalContentCrc64,
            version,
            requestId,
            leaseId0,
            maxSize,
            appendPosition,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        requestBody: {
            parameterPath: "body",
            mapper: {
                required: true,
                serializedName: "body",
                type: {
                    name: "Stream"
                }
            }
        },
        contentType: "application/octet-stream",
        responses: {
            201: {
                headersMapper: AppendBlobAppendBlockHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$5
    };
    var appendBlockFromUrlOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp15
        ],
        headerParameters: [
            sourceUrl,
            sourceRange1,
            sourceContentMD5,
            sourceContentCrc64,
            contentLength,
            transactionalContentMD5,
            version,
            requestId,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            leaseId0,
            maxSize,
            appendPosition,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch,
            sourceIfModifiedSince,
            sourceIfUnmodifiedSince,
            sourceIfMatch,
            sourceIfNoneMatch
        ],
        responses: {
            201: {
                headersMapper: AppendBlobAppendBlockFromUrlHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$5
    };

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is regenerated.
     */

    var Mappers$5 = /*#__PURE__*/Object.freeze({
        Block: Block,
        BlockBlobCommitBlockListHeaders: BlockBlobCommitBlockListHeaders,
        BlockBlobGetBlockListHeaders: BlockBlobGetBlockListHeaders,
        BlockBlobStageBlockFromURLHeaders: BlockBlobStageBlockFromURLHeaders,
        BlockBlobStageBlockHeaders: BlockBlobStageBlockHeaders,
        BlockBlobUploadHeaders: BlockBlobUploadHeaders,
        BlockList: BlockList,
        BlockLookupList: BlockLookupList,
        StorageError: StorageError
    });

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for
     * license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is
     * regenerated.
     */
    /** Class representing a BlockBlob. */
    var BlockBlob = /** @class */ (function () {
        /**
         * Create a BlockBlob.
         * @param {StorageClientContext} client Reference to the service client.
         */
        function BlockBlob(client) {
            this.client = client;
        }
        BlockBlob.prototype.upload = function (body, contentLength, options, callback) {
            return this.client.sendOperationRequest({
                body: body,
                contentLength: contentLength,
                options: options
            }, uploadOperationSpec, callback);
        };
        BlockBlob.prototype.stageBlock = function (blockId, contentLength, body, options, callback) {
            return this.client.sendOperationRequest({
                blockId: blockId,
                contentLength: contentLength,
                body: body,
                options: options
            }, stageBlockOperationSpec, callback);
        };
        BlockBlob.prototype.stageBlockFromURL = function (blockId, contentLength, sourceUrl, options, callback) {
            return this.client.sendOperationRequest({
                blockId: blockId,
                contentLength: contentLength,
                sourceUrl: sourceUrl,
                options: options
            }, stageBlockFromURLOperationSpec, callback);
        };
        BlockBlob.prototype.commitBlockList = function (blocks, options, callback) {
            return this.client.sendOperationRequest({
                blocks: blocks,
                options: options
            }, commitBlockListOperationSpec, callback);
        };
        BlockBlob.prototype.getBlockList = function (listType, options, callback) {
            return this.client.sendOperationRequest({
                listType: listType,
                options: options
            }, getBlockListOperationSpec, callback);
        };
        return BlockBlob;
    }());
    // Operation Specifications
    var serializer$6 = new Serializer(Mappers$5, true);
    var uploadOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout
        ],
        headerParameters: [
            contentLength,
            metadata,
            tier0,
            version,
            requestId,
            blobType2,
            blobContentType,
            blobContentEncoding,
            blobContentLanguage,
            blobContentMD5,
            blobCacheControl,
            blobContentDisposition,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        requestBody: {
            parameterPath: "body",
            mapper: {
                required: true,
                serializedName: "body",
                type: {
                    name: "Stream"
                }
            }
        },
        contentType: "application/octet-stream",
        responses: {
            201: {
                headersMapper: BlockBlobUploadHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$6
    };
    var stageBlockOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            blockId,
            timeout,
            comp16
        ],
        headerParameters: [
            contentLength,
            transactionalContentMD5,
            transactionalContentCrc64,
            version,
            requestId,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm
        ],
        requestBody: {
            parameterPath: "body",
            mapper: {
                required: true,
                serializedName: "body",
                type: {
                    name: "Stream"
                }
            }
        },
        contentType: "application/octet-stream",
        responses: {
            201: {
                headersMapper: BlockBlobStageBlockHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$6
    };
    var stageBlockFromURLOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            blockId,
            timeout,
            comp16
        ],
        headerParameters: [
            contentLength,
            sourceUrl,
            sourceRange1,
            sourceContentMD5,
            sourceContentCrc64,
            version,
            requestId,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            leaseId0,
            sourceIfModifiedSince,
            sourceIfUnmodifiedSince,
            sourceIfMatch,
            sourceIfNoneMatch
        ],
        responses: {
            201: {
                headersMapper: BlockBlobStageBlockFromURLHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$6
    };
    var commitBlockListOperationSpec = {
        httpMethod: "PUT",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            timeout,
            comp17
        ],
        headerParameters: [
            transactionalContentMD5,
            transactionalContentCrc64,
            metadata,
            tier0,
            version,
            requestId,
            blobCacheControl,
            blobContentType,
            blobContentEncoding,
            blobContentLanguage,
            blobContentMD5,
            blobContentDisposition,
            leaseId0,
            encryptionKey,
            encryptionKeySha256,
            encryptionAlgorithm,
            ifModifiedSince,
            ifUnmodifiedSince,
            ifMatch,
            ifNoneMatch
        ],
        requestBody: {
            parameterPath: "blocks",
            mapper: __assign({}, BlockLookupList, { required: true })
        },
        contentType: "application/xml; charset=utf-8",
        responses: {
            201: {
                headersMapper: BlockBlobCommitBlockListHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$6
    };
    var getBlockListOperationSpec = {
        httpMethod: "GET",
        path: "{containerName}/{blob}",
        urlParameters: [
            url
        ],
        queryParameters: [
            snapshot,
            listType,
            timeout,
            comp17
        ],
        headerParameters: [
            version,
            requestId,
            leaseId0
        ],
        responses: {
            200: {
                bodyMapper: BlockList,
                headersMapper: BlockBlobGetBlockListHeaders
            },
            default: {
                bodyMapper: StorageError
            }
        },
        isXML: true,
        serializer: serializer$6
    };

    /**
     * Generate a range string. For example:
     *
     * "bytes=255-" or "bytes=0-511"
     *
     * @export
     * @param {IRange} iRange
     * @returns {string}
     */
    function rangeToString(iRange) {
        if (iRange.offset < 0) {
            throw new RangeError("IRange.offset cannot be smaller than 0.");
        }
        if (iRange.count && iRange.count <= 0) {
            throw new RangeError("IRange.count must be larger than 0. Leave it undefined if you want a range from offset to the end.");
        }
        return iRange.count
            ? "bytes=" + iRange.offset + "-" + (iRange.offset + iRange.count - 1)
            : "bytes=" + iRange.offset + "-";
    }

    var BLOCK_BLOB_MAX_UPLOAD_BLOB_BYTES = 256 * 1024 * 1024; // 256MB
    var BLOCK_BLOB_MAX_STAGE_BLOCK_BYTES = 100 * 1024 * 1024; // 100MB
    var BLOCK_BLOB_MAX_BLOCKS = 50000;
    var DEFAULT_BLOB_DOWNLOAD_BLOCK_BYTES = 4 * 1024 * 1024; // 4MB
    var DEFAULT_MAX_DOWNLOAD_RETRY_REQUESTS = 5;
    var URLConstants = {
        Parameters: {
            FORCE_BROWSER_NO_CACHE: "_",
            SIGNATURE: "sig",
            SNAPSHOT: "snapshot",
            TIMEOUT: "timeout"
        }
    };
    var HTTPURLConnection = {
        HTTP_ACCEPTED: 202,
        HTTP_CONFLICT: 409,
        HTTP_NOT_FOUND: 404,
        HTTP_PRECON_FAILED: 412,
        HTTP_RANGE_NOT_SATISFIABLE: 416
    };
    var HeaderConstants = {
        AUTHORIZATION: "Authorization",
        AUTHORIZATION_SCHEME: "Bearer",
        CONTENT_ENCODING: "Content-Encoding",
        CONTENT_ID: "Content-ID",
        CONTENT_LANGUAGE: "Content-Language",
        CONTENT_LENGTH: "Content-Length",
        CONTENT_MD5: "Content-Md5",
        CONTENT_TRANSFER_ENCODING: "Content-Transfer-Encoding",
        CONTENT_TYPE: "Content-Type",
        COOKIE: "Cookie",
        DATE: "date",
        IF_MATCH: "if-match",
        IF_MODIFIED_SINCE: "if-modified-since",
        IF_NONE_MATCH: "if-none-match",
        IF_UNMODIFIED_SINCE: "if-unmodified-since",
        PREFIX_FOR_STORAGE: "x-ms-",
        RANGE: "Range",
        USER_AGENT: "User-Agent",
        X_MS_CLIENT_REQUEST_ID: "x-ms-client-request-id",
        X_MS_DATE: "x-ms-date",
        X_MS_ERROR_CODE: "x-ms-error-code",
        X_MS_VERSION: "x-ms-version"
    };
    var ETagNone = "";
    var BATCH_MAX_REQUEST = 256;
    var HTTP_LINE_ENDING = "\r\n";
    var HTTP_VERSION_1_1 = "HTTP/1.1";
    var EncryptionAlgorithmAES25 = "AES256";

    (function (BlockBlobTier) {
        BlockBlobTier["Hot"] = "Hot";
        BlockBlobTier["Cool"] = "Cool";
        BlockBlobTier["Archive"] = "Archive";
    })(exports.BlockBlobTier || (exports.BlockBlobTier = {}));
    (function (PremiumPageBlobTier) {
        PremiumPageBlobTier["P4"] = "P4";
        PremiumPageBlobTier["P6"] = "P6";
        PremiumPageBlobTier["P10"] = "P10";
        PremiumPageBlobTier["P15"] = "P15";
        PremiumPageBlobTier["P20"] = "P20";
        PremiumPageBlobTier["P30"] = "P30";
        PremiumPageBlobTier["P40"] = "P40";
        PremiumPageBlobTier["P50"] = "P50";
        PremiumPageBlobTier["P60"] = "P60";
        PremiumPageBlobTier["P70"] = "P70";
        PremiumPageBlobTier["P80"] = "P80";
    })(exports.PremiumPageBlobTier || (exports.PremiumPageBlobTier = {}));
    function toAccessTier(tier) {
        if (tier == undefined) {
            return undefined;
        }
        return tier; // No more check if string is a valid AccessTier, and left this to underlay logic to decide(service).
    }
    function ensureCpkIfSpecified(cpk, isHttps) {
        if (cpk && !isHttps) {
            throw new RangeError("Customer-provided encryption key must be used over HTTPS.");
        }
        if (cpk && !cpk.encryptionAlgorithm) {
            cpk.encryptionAlgorithm = EncryptionAlgorithmAES25;
        }
    }

    /**
     * Reserved URL characters must be properly escaped for Storage services like Blob or File.
     *
     * ## URL encode and escape strategy for JSv10 SDKs
     *
     * When customers pass a URL string into XXXURL classes constrcutor, the URL string may already be URL encoded or not.
     * But before sending to Azure Storage server, the URL must be encoded. However, it's hard for a SDK to guess whether the URL
     * string has been encoded or not. We have 2 potential strategies, and chose strategy two for the XXXURL constructors.
     *
     * ### Strategy One: Assume the customer URL string is not encoded, and always encode URL string in SDK.
     *
     * This is what legacy V2 SDK does, simple and works for most of the cases.
     * - When customer URL string is "http://account.blob.core.windows.net/con/b:",
     *   SDK will encode it to "http://account.blob.core.windows.net/con/b%3A" and send to server. A blob named "b:" will be created.
     * - When customer URL string is "http://account.blob.core.windows.net/con/b%3A",
     *   SDK will encode it to "http://account.blob.core.windows.net/con/b%253A" and send to server. A blob named "b%3A" will be created.
     *
     * But this strategy will make it not possible to create a blob with "?" in it's name. Because when customer URL string is
     * "http://account.blob.core.windows.net/con/blob?name", the "?name" will be treated as URL paramter instead of blob name.
     * If customer URL string is "http://account.blob.core.windows.net/con/blob%3Fname", a blob named "blob%3Fname" will be created.
     * V2 SDK doesn't have this issue because it doesn't allow customer pass in a full URL, it accepts a separate blob name and encodeURIComponent for it.
     * We cannot accept a SDK cannot create a blob name with "?". So we implement strategy two:
     *
     * ### Strategy Two: SDK doesn't assume the URL has been encoded or not. It will just escape the special characters.
     *
     * This is what V10 Blob Go SDK does. It accepts a URL type in Go, and call url.EscapedPath() to escape the special chars unescaped.
     * - When customer URL string is "http://account.blob.core.windows.net/con/b:",
     *   SDK will escape ":" like "http://account.blob.core.windows.net/con/b%3A" and send to server. A blob named "b:" will be created.
     * - When customer URL string is "http://account.blob.core.windows.net/con/b%3A",
     *   There is no special characters, so send "http://account.blob.core.windows.net/con/b%3A" to server. A blob named "b:" will be created.
     * - When customer URL string is "http://account.blob.core.windows.net/con/b%253A",
     *   There is no special characters, so send "http://account.blob.core.windows.net/con/b%253A" to server. A blob named "b%3A" will be created.
     *
     * This strategy gives us flexibility to create with any special characters. But "%" will be treated as a special characters, if the URL string
     * is not encoded, there shouldn't a "%" in the URL string, otherwise the URL is not a valid URL.
     * If customer needs to create a blob with "%" in it's blob name, use "%25" insead of "%". Just like above 3rd sample.
     * And following URL strings are invalid:
     * - "http://account.blob.core.windows.net/con/b%"
     * - "http://account.blob.core.windows.net/con/b%2"
     * - "http://account.blob.core.windows.net/con/b%G"
     *
     * Another special character is "?", use "%2F" to represent a blob name with "?" in a URL string.
     *
     * ### Strategy for containerName, blobName or other specific XXXName parameters in methods such as `BlobURL.fromContainerURL(containerURL, blobName)`
     *
     * We will apply strategy one, and call encodeURIComponent for these parameters like blobName. Because what customers passes in is a plain name instead of a URL.
     *
     * @see https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata
     * @see https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-shares--directories--files--and-metadata
     *
     * @export
     * @param {string} url
     * @returns {string}
     */
    function escapeURLPath(url) {
        var urlParsed = URLBuilder.parse(url);
        var path = urlParsed.getPath();
        path = path || "/";
        path = escape(path);
        urlParsed.setPath(path);
        return urlParsed.toString();
    }
    /**
     * Internal escape method implmented Strategy Two mentioned in escapeURL() description.
     *
     * @param {string} text
     * @returns {string}
     */
    function escape(text) {
        return encodeURIComponent(text)
            .replace(/%2F/g, "/") // Don't escape for "/"
            .replace(/'/g, "%27") // Escape for "'"
            .replace(/\+/g, "%20")
            .replace(/%25/g, "%"); // Revert encoded "%"
    }
    /**
     * Append a string to URL path. Will remove duplicated "/" in front of the string
     * when URL path ends with a "/".
     *
     * @export
     * @param {string} url Source URL string
     * @param {string} name String to be appended to URL
     * @returns {string} An updated URL string
     */
    function appendToURLPath(url, name) {
        var urlParsed = URLBuilder.parse(url);
        var path = urlParsed.getPath();
        path = path ? (path.endsWith("/") ? "" + path + name : path + "/" + name) : name;
        urlParsed.setPath(path);
        return urlParsed.toString();
    }
    /**
     * Set URL parameter name and value. If name exists in URL parameters, old value
     * will be replaced by name key. If not provide value, the parameter will be deleted.
     *
     * @export
     * @param {string} url Source URL string
     * @param {string} name Parameter name
     * @param {string} [value] Parameter value
     * @returns {string} An updated URL string
     */
    function setURLParameter(url, name, value) {
        var urlParsed = URLBuilder.parse(url);
        urlParsed.setQueryParameter(name, value);
        return urlParsed.toString();
    }
    /**
     * Get URL parameter by name.
     *
     * @export
     * @param {string} url
     * @param {string} name
     * @returns {(string | string[] | undefined)}
     */
    function getURLParameter(url, name) {
        var urlParsed = URLBuilder.parse(url);
        return urlParsed.getQueryParameterValue(name);
    }
    /**
     * Set URL host.
     *
     * @export
     * @param {string} url Source URL string
     * @param {string} host New host string
     * @returns An updated URL string
     */
    function setURLHost(url, host) {
        var urlParsed = URLBuilder.parse(url);
        urlParsed.setHost(host);
        return urlParsed.toString();
    }
    /**
     * Get URL path from an URL string.
     *
     * @export
     * @param {string} url Source URL string
     * @returns {(string | undefined)}
     */
    function getURLPath(url) {
        var urlParsed = URLBuilder.parse(url);
        return urlParsed.getPath();
    }
    /**
     * Get URL scheme from an URL string.
     *
     * @export
     * @param {string} url Source URL string
     * @returns {(string | undefined)}
     */
    function getURLScheme(url) {
        var urlParsed = URLBuilder.parse(url);
        return urlParsed.getScheme();
    }
    /**
     * Get URL path and query from an URL string.
     *
     * @export
     * @param {string} url Source URL string
     * @returns {(string | undefined)}
     */
    function getURLPathAndQuery(url) {
        var urlParsed = URLBuilder.parse(url);
        var pathString = urlParsed.getPath();
        if (!pathString) {
            throw new RangeError("Invalid url without valid path.");
        }
        var queryString = urlParsed.getQuery() || "";
        queryString = queryString.trim();
        if (queryString != "") {
            queryString = queryString.startsWith("?") ? queryString : "?" + queryString; // Ensure query string start with '?'
        }
        return "" + pathString + queryString;
    }
    /**
     * Rounds a date off to seconds.
     *
     * @export
     * @param {Date} date
     * @param {boolean} [withMilliseconds=true] If true, YYYY-MM-DDThh:mm:ss.fffffffZ will be returned;
     *                                          If false, YYYY-MM-DDThh:mm:ssZ will be returned.
     * @returns {string} Date string in ISO8061 format, with or without 7 milliseconds component
     */
    function truncatedISO8061Date(date, withMilliseconds) {
        if (withMilliseconds === void 0) { withMilliseconds = true; }
        // Date.toISOString() will return like "2018-10-29T06:34:36.139Z"
        var dateString = date.toISOString();
        return withMilliseconds
            ? dateString.substring(0, dateString.length - 1) + "0000" + "Z"
            : dateString.substring(0, dateString.length - 5) + "Z";
    }
    /**
     * Base64 encode.
     *
     * @export
     * @param {string} content
     * @returns {string}
     */
    function base64encode(content) {
        return !isNode ? btoa(content) : Buffer.from(content).toString("base64");
    }
    /**
     * Generate a 64 bytes base64 block ID string.
     *
     * @export
     * @param {number} blockIndex
     * @returns {string}
     */
    function generateBlockID(blockIDPrefix, blockIndex) {
        // To generate a 64 bytes base64 string, source string should be 48
        var maxSourceStringLength = 48;
        // A blob can have a maximum of 100,000 uncommitted blocks at any given time
        var maxBlockIndexLength = 6;
        var maxAllowedBlockIDPrefixLength = maxSourceStringLength - maxBlockIndexLength;
        if (blockIDPrefix.length > maxAllowedBlockIDPrefixLength) {
            blockIDPrefix = blockIDPrefix.slice(0, maxAllowedBlockIDPrefixLength);
        }
        var res = blockIDPrefix +
            padStart(blockIndex.toString(), maxSourceStringLength - blockIDPrefix.length, "0");
        return base64encode(res);
    }
    /**
     * Delay specified time interval.
     *
     * @export
     * @param {number} timeInMs
     * @param {AbortSignalLike} [aborter]
     * @param {Error} [abortError]
     */
    function delay$1(timeInMs, aborter, abortError) {
        return __awaiter(this, void 0, void 0, function () {
            return __generator(this, function (_a) {
                return [2 /*return*/, new Promise(function (resolve, reject) {
                        var timeout;
                        var abortHandler = function () {
                            if (timeout !== undefined) {
                                clearTimeout(timeout);
                            }
                            reject(abortError);
                        };
                        var resolveHandler = function () {
                            if (aborter !== undefined) {
                                aborter.removeEventListener("abort", abortHandler);
                            }
                            resolve();
                        };
                        timeout = setTimeout(resolveHandler, timeInMs);
                        if (aborter !== undefined) {
                            aborter.addEventListener("abort", abortHandler);
                        }
                    })];
            });
        });
    }
    /**
     * String.prototype.padStart()
     *
     * @export
     * @param {string} currentString
     * @param {number} targetLength
     * @param {string} [padString=" "]
     * @returns {string}
     */
    function padStart(currentString, targetLength, padString) {
        if (padString === void 0) { padString = " "; }
        if (String.prototype.padStart) {
            return currentString.padStart(targetLength, padString);
        }
        padString = padString || " ";
        if (currentString.length > targetLength) {
            return currentString;
        }
        else {
            targetLength = targetLength - currentString.length;
            if (targetLength > padString.length) {
                padString += padString.repeat(targetLength / padString.length);
            }
            return padString.slice(0, targetLength) + currentString;
        }
    }
    /**
     * If two strings are equal when compared case insensitive.
     *
     * @export
     * @param {string} str1
     * @param {string} str2
     * @returns {boolean}
     */
    function iEqual(str1, str2) {
        return str1.toLocaleLowerCase() === str2.toLocaleLowerCase();
    }

    /**
     * BrowserPolicy will handle differences between Node.js and browser runtime, including:
     *
     * 1. Browsers cache GET/HEAD requests by adding conditional headers such as 'IF_MODIFIED_SINCE'.
     * BrowserPolicy is a policy used to add a timestamp query to GET/HEAD request URL
     * thus avoid the browser cache.
     *
     * 2. Remove cookie header for security
     *
     * 3. Remove content-length header to avoid browsers warning
     *
     * @class BrowserPolicy
     * @extends {BaseRequestPolicy}
     */
    var BrowserPolicy = /** @class */ (function (_super) {
        __extends(BrowserPolicy, _super);
        /**
         * Creates an instance of BrowserPolicy.
         * @param {RequestPolicy} nextPolicy
         * @param {RequestPolicyOptions} options
         * @memberof BrowserPolicy
         */
        function BrowserPolicy(nextPolicy, options) {
            return _super.call(this, nextPolicy, options) || this;
        }
        /**
         * Sends out request.
         *
         * @param {WebResource} request
         * @returns {Promise<HttpOperationResponse>}
         * @memberof BrowserPolicy
         */
        BrowserPolicy.prototype.sendRequest = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    if (request.method.toUpperCase() === "GET" || request.method.toUpperCase() === "HEAD") {
                        request.url = setURLParameter(request.url, URLConstants.Parameters.FORCE_BROWSER_NO_CACHE, new Date().getTime().toString());
                    }
                    request.headers.remove(HeaderConstants.COOKIE);
                    // According to XHR standards, content-length should be fully controlled by browsers
                    request.headers.remove(HeaderConstants.CONTENT_LENGTH);
                    return [2 /*return*/, this._nextPolicy.sendRequest(request)];
                });
            });
        };
        return BrowserPolicy;
    }(BaseRequestPolicy));

    /**
     * BrowserPolicyFactory is a factory class helping generating BrowserPolicy objects.
     *
     * @export
     * @class BrowserPolicyFactory
     * @implements {RequestPolicyFactory}
     */
    var BrowserPolicyFactory = /** @class */ (function () {
        function BrowserPolicyFactory() {
        }
        BrowserPolicyFactory.prototype.create = function (nextPolicy, options) {
            return new BrowserPolicy(nextPolicy, options);
        };
        return BrowserPolicyFactory;
    }());

    /**
     * Credential is an abstract class for Azure Storage HTTP requests signing. This
     * class will host an credentialPolicyCreator factory which generates CredentialPolicy.
     *
     * @export
     * @abstract
     * @class Credential
     */
    var Credential = /** @class */ (function () {
        function Credential() {
        }
        /**
         * Creates a RequestPolicy object.
         *
         * @param {RequestPolicy} _nextPolicy
         * @param {RequestPolicyOptions} _options
         * @returns {RequestPolicy}
         * @memberof Credential
         */
        Credential.prototype.create = function (
        // tslint:disable-next-line:variable-name
        _nextPolicy, 
        // tslint:disable-next-line:variable-name
        _options) {
            throw new Error("Method should be implemented in children classes.");
        };
        return Credential;
    }());

    /*
     * Copyright (c) Microsoft Corporation. All rights reserved.
     * Licensed under the MIT License. See License.txt in the project root for
     * license information.
     *
     * Code generated by Microsoft (R) AutoRest Code Generator.
     * Changes may cause incorrect behavior and will be lost if the code is
     * regenerated.
     */
    var packageName = "azure-storage-blob";
    var packageVersion = "1.0.0";
    var StorageClientContext = /** @class */ (function (_super) {
        __extends(StorageClientContext, _super);
        /**
         * Initializes a new instance of the StorageClientContext class.
         * @param url The URL of the service account, container, or blob that is the targe of the desired
         * operation.
         * @param [options] The parameter options
         */
        function StorageClientContext(url, options) {
            var _this = this;
            if (url == undefined) {
                throw new Error("'url' cannot be null.");
            }
            if (!options) {
                options = {};
            }
            if (!options.userAgent) {
                var defaultUserAgent = getDefaultUserAgentValue();
                options.userAgent = packageName + "/" + packageVersion + " " + defaultUserAgent;
            }
            _this = _super.call(this, undefined, options) || this;
            _this.version = '2019-02-02';
            _this.baseUri = "{url}";
            _this.requestContentType = "application/json; charset=utf-8";
            _this.url = url;
            if (options.pathRenameMode !== null && options.pathRenameMode !== undefined) {
                _this.pathRenameMode = options.pathRenameMode;
            }
            return _this;
        }
        return StorageClientContext;
    }(ServiceClient));

    /**
     * KeepAlivePolicy is a policy used to control keep alive settings for every request.
     *
     * @class KeepAlivePolicy
     * @extends {BaseRequestPolicy}
     */
    var KeepAlivePolicy = /** @class */ (function (_super) {
        __extends(KeepAlivePolicy, _super);
        /**
         * Creates an instance of KeepAlivePolicy.
         *
         * @param {RequestPolicy} nextPolicy
         * @param {RequestPolicyOptions} options
         * @param {IKeepAliveOptions} [keepAliveOptions]
         * @memberof KeepAlivePolicy
         */
        function KeepAlivePolicy(nextPolicy, options, keepAliveOptions) {
            var _this = _super.call(this, nextPolicy, options) || this;
            _this.keepAliveOptions = keepAliveOptions;
            return _this;
        }
        /**
         * Sends out request.
         *
         * @param {WebResource} request
         * @returns {Promise<HttpOperationResponse>}
         * @memberof KeepAlivePolicy
         */
        KeepAlivePolicy.prototype.sendRequest = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    request.keepAlive = this.keepAliveOptions.enable;
                    return [2 /*return*/, this._nextPolicy.sendRequest(request)];
                });
            });
        };
        return KeepAlivePolicy;
    }(BaseRequestPolicy));

    /**
     * KeepAlivePolicyFactory is a factory class helping generating KeepAlivePolicy objects.
     *
     * @export
     * @class KeepAlivePolicyFactory
     * @implements {RequestPolicyFactory}
     */
    var KeepAlivePolicyFactory = /** @class */ (function () {
        /**
         * Creates an instance of KeepAlivePolicyFactory.
         *
         * @param {IKeepAliveOptions} [telemetry]
         * @memberof KeepAlivePolicyFactory
         */
        function KeepAlivePolicyFactory(keepAliveOptions) {
            if (keepAliveOptions === void 0) { keepAliveOptions = { enable: true }; }
            this.keepAliveOptions = keepAliveOptions;
        }
        KeepAlivePolicyFactory.prototype.create = function (nextPolicy, options) {
            return new KeepAlivePolicy(nextPolicy, options, this.keepAliveOptions);
        };
        return KeepAlivePolicyFactory;
    }());

    // Default values of IRetryOptions
    var DEFAULT_REQUEST_LOG_OPTIONS = {
        logWarningIfTryOverThreshold: 3000
    };
    /**
     * LoggingPolicy is a policy used to log requests.
     *
     * @class LoggingPolicy
     * @extends {BaseRequestPolicy}
     */
    var LoggingPolicy = /** @class */ (function (_super) {
        __extends(LoggingPolicy, _super);
        /**
         * Creates an instance of LoggingPolicy.
         * @param {RequestPolicy} nextPolicy
         * @param {RequestPolicyOptions} options
         * @param {IRequestLogOptions} [loggingOptions=DEFAULT_REQUEST_LOG_OPTIONS]
         * @memberof LoggingPolicy
         */
        function LoggingPolicy(nextPolicy, options, loggingOptions) {
            if (loggingOptions === void 0) { loggingOptions = DEFAULT_REQUEST_LOG_OPTIONS; }
            var _this = _super.call(this, nextPolicy, options) || this;
            _this.tryCount = 0;
            _this.operationStartTime = new Date();
            _this.requestStartTime = new Date();
            _this.loggingOptions = loggingOptions;
            return _this;
        }
        /**
         * Sends out request.
         *
         * @param {WebResource} request
         * @returns {Promise<HttpOperationResponse>}
         * @memberof LoggingPolicy
         */
        LoggingPolicy.prototype.sendRequest = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                var safeURL, response, requestEndTime, requestCompletionTime, operationDuration, currentLevel, logMessage, errorString, messageInfo, err_1;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            this.tryCount++;
                            this.requestStartTime = new Date();
                            if (this.tryCount === 1) {
                                this.operationStartTime = this.requestStartTime;
                            }
                            safeURL = request.url;
                            if (getURLParameter(safeURL, URLConstants.Parameters.SIGNATURE)) {
                                safeURL = setURLParameter(safeURL, URLConstants.Parameters.SIGNATURE, "*****");
                            }
                            this.log(exports.HttpPipelineLogLevel.INFO, "'" + safeURL + "'==> OUTGOING REQUEST (Try number=" + this.tryCount + ").");
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 3, , 4]);
                            return [4 /*yield*/, this._nextPolicy.sendRequest(request)];
                        case 2:
                            response = _a.sent();
                            requestEndTime = new Date();
                            requestCompletionTime = requestEndTime.getTime() - this.requestStartTime.getTime();
                            operationDuration = requestEndTime.getTime() - this.operationStartTime.getTime();
                            currentLevel = exports.HttpPipelineLogLevel.INFO;
                            logMessage = "";
                            if (this.shouldLog(exports.HttpPipelineLogLevel.INFO)) {
                                // Assume success and default to informational logging.
                                logMessage = "Successfully Received Response. ";
                            }
                            // If the response took too long, we'll upgrade to warning.
                            if (requestCompletionTime >= this.loggingOptions.logWarningIfTryOverThreshold) {
                                // Log a warning if the try duration exceeded the specified threshold.
                                if (this.shouldLog(exports.HttpPipelineLogLevel.WARNING)) {
                                    currentLevel = exports.HttpPipelineLogLevel.WARNING;
                                    logMessage = "SLOW OPERATION. Duration > " + this.loggingOptions.logWarningIfTryOverThreshold + " ms. ";
                                }
                            }
                            if ((response.status >= 400 &&
                                response.status <= 499 &&
                                (response.status !== HTTPURLConnection.HTTP_NOT_FOUND &&
                                    response.status !== HTTPURLConnection.HTTP_CONFLICT &&
                                    response.status !== HTTPURLConnection.HTTP_PRECON_FAILED &&
                                    response.status !== HTTPURLConnection.HTTP_RANGE_NOT_SATISFIABLE)) ||
                                (response.status >= 500 && response.status <= 509)) {
                                errorString = "REQUEST ERROR: HTTP request failed with status code: " + response.status + ". ";
                                logMessage = errorString;
                                currentLevel = exports.HttpPipelineLogLevel.ERROR;
                            }
                            messageInfo = "Request try:" + this.tryCount + ", status:" + response.status + " request duration:" + requestCompletionTime + " ms, operation duration:" + operationDuration + " ms\n";
                            this.log(currentLevel, logMessage + messageInfo);
                            return [2 /*return*/, response];
                        case 3:
                            err_1 = _a.sent();
                            this.log(exports.HttpPipelineLogLevel.ERROR, "Unexpected failure attempting to make request. Error message: " + err_1.message);
                            throw err_1;
                        case 4: return [2 /*return*/];
                    }
                });
            });
        };
        return LoggingPolicy;
    }(BaseRequestPolicy));

    /**
     * LoggingPolicyFactory is a factory class helping generating LoggingPolicy objects.
     *
     * @export
     * @class LoggingPolicyFactory
     * @implements {RequestPolicyFactory}
     */
    var LoggingPolicyFactory = /** @class */ (function () {
        function LoggingPolicyFactory(loggingOptions) {
            this.loggingOptions = loggingOptions;
        }
        LoggingPolicyFactory.prototype.create = function (nextPolicy, options) {
            return new LoggingPolicy(nextPolicy, options, this.loggingOptions);
        };
        return LoggingPolicyFactory;
    }());

    /**
     * A Pipeline class containing HTTP request policies.
     * You can create a default Pipeline by calling StorageURL.newPipeline().
     * Or you can create a Pipeline with your own policies by the constructor of Pipeline.
     * Refer to StorageURL.newPipeline() and provided policies as reference before
     * implementing your customized Pipeline.
     *
     * @export
     * @class Pipeline
     */
    var Pipeline = /** @class */ (function () {
        /**
         * Creates an instance of Pipeline. Customize HTTPClient by implementing IHttpClient interface.
         *
         * @param {RequestPolicyFactory[]} factories
         * @param {IPipelineOptions} [options={}]
         * @memberof Pipeline
         */
        function Pipeline(factories, options) {
            if (options === void 0) { options = {}; }
            this.factories = factories;
            this.options = options;
        }
        /**
         * Transfer Pipeline object to ServiceClientOptions object which required by
         * ServiceClient constructor.
         *
         * @returns {ServiceClientOptions}
         * @memberof Pipeline
         */
        Pipeline.prototype.toServiceClientOptions = function () {
            return {
                httpClient: this.options.HTTPClient,
                httpPipelineLogger: this.options.logger,
                requestPolicyFactories: this.factories
            };
        };
        return Pipeline;
    }());

    (function (RetryPolicyType) {
        /**
         * Exponential retry. Retry time delay grows exponentially.
         */
        RetryPolicyType[RetryPolicyType["EXPONENTIAL"] = 0] = "EXPONENTIAL";
        /**
         * Linear retry. Retry time delay grows linearly.
         */
        RetryPolicyType[RetryPolicyType["FIXED"] = 1] = "FIXED";
    })(exports.RetryPolicyType || (exports.RetryPolicyType = {}));
    // Default values of IRetryOptions
    var DEFAULT_RETRY_OPTIONS = {
        maxRetryDelayInMs: 120 * 1000,
        maxTries: 4,
        retryDelayInMs: 4 * 1000,
        retryPolicyType: exports.RetryPolicyType.EXPONENTIAL,
        secondaryHost: "",
        tryTimeoutInMs: undefined // Use server side default timeout strategy
    };
    var RETRY_ABORT_ERROR = new RestError("The request was aborted", RestError.REQUEST_ABORTED_ERROR);
    /**
     * Retry policy with exponential retry and linear retry implemented.
     *
     * @class RetryPolicy
     * @extends {BaseRequestPolicy}
     */
    var RetryPolicy = /** @class */ (function (_super) {
        __extends(RetryPolicy, _super);
        /**
         * Creates an instance of RetryPolicy.
         *
         * @param {RequestPolicy} nextPolicy
         * @param {RequestPolicyOptions} options
         * @param {IRetryOptions} [retryOptions=DEFAULT_RETRY_OPTIONS]
         * @memberof RetryPolicy
         */
        function RetryPolicy(nextPolicy, options, retryOptions) {
            if (retryOptions === void 0) { retryOptions = DEFAULT_RETRY_OPTIONS; }
            var _this = _super.call(this, nextPolicy, options) || this;
            // Initialize retry options
            _this.retryOptions = {
                retryPolicyType: retryOptions.retryPolicyType
                    ? retryOptions.retryPolicyType
                    : DEFAULT_RETRY_OPTIONS.retryPolicyType,
                maxTries: retryOptions.maxTries && retryOptions.maxTries >= 1
                    ? Math.floor(retryOptions.maxTries)
                    : DEFAULT_RETRY_OPTIONS.maxTries,
                tryTimeoutInMs: retryOptions.tryTimeoutInMs && retryOptions.tryTimeoutInMs >= 0
                    ? retryOptions.tryTimeoutInMs
                    : DEFAULT_RETRY_OPTIONS.tryTimeoutInMs,
                retryDelayInMs: retryOptions.retryDelayInMs && retryOptions.retryDelayInMs >= 0
                    ? Math.min(retryOptions.retryDelayInMs, retryOptions.maxRetryDelayInMs
                        ? retryOptions.maxRetryDelayInMs
                        : DEFAULT_RETRY_OPTIONS.maxRetryDelayInMs)
                    : DEFAULT_RETRY_OPTIONS.retryDelayInMs,
                maxRetryDelayInMs: retryOptions.maxRetryDelayInMs && retryOptions.maxRetryDelayInMs >= 0
                    ? retryOptions.maxRetryDelayInMs
                    : DEFAULT_RETRY_OPTIONS.maxRetryDelayInMs,
                secondaryHost: retryOptions.secondaryHost
                    ? retryOptions.secondaryHost
                    : DEFAULT_RETRY_OPTIONS.secondaryHost
            };
            return _this;
        }
        /**
         * Sends request.
         *
         * @param {WebResource} request
         * @returns {Promise<HttpOperationResponse>}
         * @memberof RetryPolicy
         */
        RetryPolicy.prototype.sendRequest = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.attemptSendRequest(request, false, 1)];
                });
            });
        };
        /**
         * Decide and perform next retry. Won't mutate request parameter.
         *
         * @protected
         * @param {WebResource} request
         * @param {HttpOperationResponse} response
         * @param {boolean} secondaryHas404  If attempt was against the secondary & it returned a StatusNotFound (404), then
         *                                   the resource was not found. This may be due to replication delay. So, in this
         *                                   case, we'll never try the secondary again for this operation.
         * @param {number} attempt           How many retries has been attempted to performed, starting from 1, which includes
         *                                   the attempt will be performed by this method call.
         * @returns {Promise<HttpOperationResponse>}
         * @memberof RetryPolicy
         */
        RetryPolicy.prototype.attemptSendRequest = function (request, secondaryHas404, attempt) {
            return __awaiter(this, void 0, void 0, function () {
                var newRequest, isPrimaryRetry, response, err_1;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            newRequest = request.clone();
                            isPrimaryRetry = secondaryHas404 ||
                                !this.retryOptions.secondaryHost ||
                                !(request.method === "GET" || request.method === "HEAD" || request.method === "OPTIONS") ||
                                attempt % 2 === 1;
                            if (!isPrimaryRetry) {
                                newRequest.url = setURLHost(newRequest.url, this.retryOptions.secondaryHost);
                            }
                            // Set the server-side timeout query parameter "timeout=[seconds]"
                            if (this.retryOptions.tryTimeoutInMs) {
                                newRequest.url = setURLParameter(newRequest.url, URLConstants.Parameters.TIMEOUT, Math.floor(this.retryOptions.tryTimeoutInMs / 1000).toString());
                            }
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 3, , 4]);
                            this.logf(exports.HttpPipelineLogLevel.INFO, "RetryPolicy: =====> Try=" + attempt + " " + (isPrimaryRetry ? "Primary" : "Secondary"));
                            return [4 /*yield*/, this._nextPolicy.sendRequest(newRequest)];
                        case 2:
                            response = _a.sent();
                            if (!this.shouldRetry(isPrimaryRetry, attempt, response)) {
                                return [2 /*return*/, response];
                            }
                            secondaryHas404 = secondaryHas404 || (!isPrimaryRetry && response.status === 404);
                            return [3 /*break*/, 4];
                        case 3:
                            err_1 = _a.sent();
                            this.logf(exports.HttpPipelineLogLevel.ERROR, "RetryPolicy: Caught error, message: " + err_1.message + ", code: " + err_1.code);
                            if (!this.shouldRetry(isPrimaryRetry, attempt, response, err_1)) {
                                throw err_1;
                            }
                            return [3 /*break*/, 4];
                        case 4: return [4 /*yield*/, this.delay(isPrimaryRetry, attempt, request.abortSignal)];
                        case 5:
                            _a.sent();
                            return [4 /*yield*/, this.attemptSendRequest(request, secondaryHas404, ++attempt)];
                        case 6: return [2 /*return*/, _a.sent()];
                    }
                });
            });
        };
        /**
         * Decide whether to retry according to last HTTP response and retry counters.
         *
         * @protected
         * @param {boolean} isPrimaryRetry
         * @param {number} attempt
         * @param {HttpOperationResponse} [response]
         * @param {RestError} [err]
         * @returns {boolean}
         * @memberof RetryPolicy
         */
        RetryPolicy.prototype.shouldRetry = function (isPrimaryRetry, attempt, response, err) {
            if (attempt >= this.retryOptions.maxTries) {
                this.logf(exports.HttpPipelineLogLevel.INFO, "RetryPolicy: Attempt(s) " + attempt + " >= maxTries " + this.retryOptions
                    .maxTries + ", no further try.");
                return false;
            }
            // Handle network failures, you may need to customize the list when you implement
            // your own http client
            var retriableErrors = [
                "ETIMEDOUT",
                "ESOCKETTIMEDOUT",
                "ECONNREFUSED",
                "ECONNRESET",
                "ENOENT",
                "ENOTFOUND",
                "TIMEOUT",
                "REQUEST_SEND_ERROR" // For default xhr based http client provided in ms-rest-js
            ];
            if (err) {
                for (var _i = 0, retriableErrors_1 = retriableErrors; _i < retriableErrors_1.length; _i++) {
                    var retriableError = retriableErrors_1[_i];
                    if (err.name.toUpperCase().includes(retriableError) ||
                        err.message.toUpperCase().includes(retriableError) ||
                        (err.code && err.code.toString().toUpperCase().includes(retriableError))) {
                        this.logf(exports.HttpPipelineLogLevel.INFO, "RetryPolicy: Network error " + retriableError + " found, will retry.");
                        return true;
                    }
                }
            }
            // If attempt was against the secondary & it returned a StatusNotFound (404), then
            // the resource was not found. This may be due to replication delay. So, in this
            // case, we'll never try the secondary again for this operation.
            if (response || err) {
                var statusCode = response ? response.status : err ? err.statusCode : 0;
                if (!isPrimaryRetry && statusCode === 404) {
                    this.logf(exports.HttpPipelineLogLevel.INFO, "RetryPolicy: Secondary access with 404, will retry.");
                    return true;
                }
                // Server internal error or server timeout
                if (statusCode === 503 || statusCode === 500) {
                    this.logf(exports.HttpPipelineLogLevel.INFO, "RetryPolicy: Will retry for status code " + statusCode + ".");
                    return true;
                }
            }
            return false;
        };
        /**
         * This is to log for debugging purposes only.
         * Comment/uncomment as necessary for releasing/debugging.
         *
         * @private
         * @param {HttpPipelineLogLevel} level
         * @param {string} message
         * @memberof RetryPolicy
         */
        // tslint:disable-next-line:variable-name
        RetryPolicy.prototype.logf = function (_level, _message) {
            // this.log(_level, _message);
        };
        /**
         * Delay a calculated time between retries.
         *
         * @private
         * @param {boolean} isPrimaryRetry
         * @param {number} attempt
         * @param {AbortSignalLike} [abortSignal]
         * @returns
         * @memberof RetryPolicy
         */
        RetryPolicy.prototype.delay = function (isPrimaryRetry, attempt, abortSignal) {
            return __awaiter(this, void 0, void 0, function () {
                var delayTimeInMs;
                return __generator(this, function (_a) {
                    delayTimeInMs = 0;
                    if (isPrimaryRetry) {
                        switch (this.retryOptions.retryPolicyType) {
                            case exports.RetryPolicyType.EXPONENTIAL:
                                delayTimeInMs = Math.min((Math.pow(2, attempt - 1) - 1) * this.retryOptions.retryDelayInMs, this.retryOptions.maxRetryDelayInMs);
                                break;
                            case exports.RetryPolicyType.FIXED:
                                delayTimeInMs = this.retryOptions.retryDelayInMs;
                                break;
                        }
                    }
                    else {
                        delayTimeInMs = Math.random() * 1000;
                    }
                    this.logf(exports.HttpPipelineLogLevel.INFO, "RetryPolicy: Delay for " + delayTimeInMs + "ms");
                    return [2 /*return*/, delay$1(delayTimeInMs, abortSignal, RETRY_ABORT_ERROR)];
                });
            });
        };
        return RetryPolicy;
    }(BaseRequestPolicy));

    /**
     * RetryPolicyFactory is a factory class helping generating RetryPolicy objects.
     *
     * @export
     * @class RetryPolicyFactory
     * @implements {RequestPolicyFactory}
     */
    var RetryPolicyFactory = /** @class */ (function () {
        /**
         * Creates an instance of RetryPolicyFactory.
         * @param {IRetryOptions} [retryOptions]
         * @memberof RetryPolicyFactory
         */
        function RetryPolicyFactory(retryOptions) {
            this.retryOptions = retryOptions;
        }
        RetryPolicyFactory.prototype.create = function (nextPolicy, options) {
            return new RetryPolicy(nextPolicy, options, this.retryOptions);
        };
        return RetryPolicyFactory;
    }());

    /**
     * TelemetryPolicy is a policy used to tag user-agent header for every requests.
     *
     * @class TelemetryPolicy
     * @extends {BaseRequestPolicy}
     */
    var TelemetryPolicy = /** @class */ (function (_super) {
        __extends(TelemetryPolicy, _super);
        /**
         * Creates an instance of TelemetryPolicy.
         * @param {RequestPolicy} nextPolicy
         * @param {RequestPolicyOptions} options
         * @param {ITelemetryOptions} [telemetry]
         * @memberof TelemetryPolicy
         */
        function TelemetryPolicy(nextPolicy, options, telemetry) {
            var _this = _super.call(this, nextPolicy, options) || this;
            _this.telemetry = telemetry;
            return _this;
        }
        /**
         * Sends out request.
         *
         * @param {WebResource} request
         * @returns {Promise<HttpOperationResponse>}
         * @memberof TelemetryPolicy
         */
        TelemetryPolicy.prototype.sendRequest = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this._nextPolicy.sendRequest(request)];
                });
            });
        };
        return TelemetryPolicy;
    }(BaseRequestPolicy));

    /**
     * TelemetryPolicyFactory is a factory class helping generating TelemetryPolicy objects.
     *
     * @export
     * @class TelemetryPolicyFactory
     * @implements {RequestPolicyFactory}
     */
    var TelemetryPolicyFactory = /** @class */ (function () {
        /**
         * Creates an instance of TelemetryPolicyFactory.
         * @param {ITelemetryOptions} [telemetry]
         * @memberof TelemetryPolicyFactory
         */
        function TelemetryPolicyFactory(telemetry) {
            var userAgentInfo = [];
            this.telemetryString = userAgentInfo.join(" ");
        }
        TelemetryPolicyFactory.prototype.create = function (nextPolicy, options) {
            return new TelemetryPolicy(nextPolicy, options, this.telemetryString);
        };
        return TelemetryPolicyFactory;
    }());

    /**
     * UniqueRequestIDPolicy generates an UUID as x-ms-request-id header value.
     *
     * @class UniqueRequestIDPolicy
     * @extends {BaseRequestPolicy}
     */
    var UniqueRequestIDPolicy = /** @class */ (function (_super) {
        __extends(UniqueRequestIDPolicy, _super);
        /**
         * Creates an instance of UniqueRequestIDPolicy.
         * @param {RequestPolicy} nextPolicy
         * @param {RequestPolicyOptions} options
         * @memberof UniqueRequestIDPolicy
         */
        function UniqueRequestIDPolicy(nextPolicy, options) {
            return _super.call(this, nextPolicy, options) || this;
        }
        /**
         * Sends request.
         *
         * @param {WebResource} request
         * @returns {Promise<HttpOperationResponse>}
         * @memberof UniqueRequestIDPolicy
         */
        UniqueRequestIDPolicy.prototype.sendRequest = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    if (!request.headers.contains(HeaderConstants.X_MS_CLIENT_REQUEST_ID)) {
                        request.headers.set(HeaderConstants.X_MS_CLIENT_REQUEST_ID, generateUuid());
                    }
                    return [2 /*return*/, this._nextPolicy.sendRequest(request)];
                });
            });
        };
        return UniqueRequestIDPolicy;
    }(BaseRequestPolicy));

    /**
     * UniqueRequestIDPolicyFactory is a factory class helping generating UniqueRequestIDPolicy objects.
     *
     * @export
     * @class UniqueRequestIDPolicyFactory
     * @implements {RequestPolicyFactory}
     */
    var UniqueRequestIDPolicyFactory = /** @class */ (function () {
        function UniqueRequestIDPolicyFactory() {
        }
        UniqueRequestIDPolicyFactory.prototype.create = function (nextPolicy, options) {
            return new UniqueRequestIDPolicy(nextPolicy, options);
        };
        return UniqueRequestIDPolicyFactory;
    }());

    /**
     * Credential policy used to sign HTTP(S) requests before sending. This is an
     * abstract class.
     *
     * @export
     * @abstract
     * @class CredentialPolicy
     * @extends {BaseRequestPolicy}
     */
    var CredentialPolicy = /** @class */ (function (_super) {
        __extends(CredentialPolicy, _super);
        function CredentialPolicy() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        /**
         * Sends out request.
         *
         * @param {WebResource} request
         * @returns {Promise<HttpOperationResponse>}
         * @memberof CredentialPolicy
         */
        CredentialPolicy.prototype.sendRequest = function (request) {
            return this._nextPolicy.sendRequest(this.signRequest(request));
        };
        /**
         * Child classes must implement this method with request signing. This method
         * will be executed in sendRequest().
         *
         * @protected
         * @abstract
         * @param {WebResource} request
         * @returns {WebResource}
         * @memberof CredentialPolicy
         */
        CredentialPolicy.prototype.signRequest = function (request) {
            // Child classes must override this method with request signing. This method
            // will be executed in sendRequest().
            return request;
        };
        return CredentialPolicy;
    }(BaseRequestPolicy));

    /**
     * AnonymousCredentialPolicy is used with HTTP(S) requests that read public resources
     * or for use with Shared Access Signatures (SAS).
     *
     * @export
     * @class AnonymousCredentialPolicy
     * @extends {CredentialPolicy}
     */
    var AnonymousCredentialPolicy = /** @class */ (function (_super) {
        __extends(AnonymousCredentialPolicy, _super);
        /**
         * Creates an instance of AnonymousCredentialPolicy.
         * @param {RequestPolicy} nextPolicy
         * @param {RequestPolicyOptions} options
         * @memberof AnonymousCredentialPolicy
         */
        function AnonymousCredentialPolicy(nextPolicy, options) {
            return _super.call(this, nextPolicy, options) || this;
        }
        return AnonymousCredentialPolicy;
    }(CredentialPolicy));

    /**
     * AnonymousCredential provides a credentialPolicyCreator member used to create
     * AnonymousCredentialPolicy objects. AnonymousCredentialPolicy is used with
     * HTTP(S) requests that read public resources or for use with Shared Access
     * Signatures (SAS).
     *
     * @export
     * @class AnonymousCredential
     * @extends {Credential}
     */
    var AnonymousCredential = /** @class */ (function (_super) {
        __extends(AnonymousCredential, _super);
        function AnonymousCredential() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        /**
         * Creates an AnonymousCredentialPolicy object.
         *
         * @param {RequestPolicy} nextPolicy
         * @param {RequestPolicyOptions} options
         * @returns {AnonymousCredentialPolicy}
         * @memberof AnonymousCredential
         */
        AnonymousCredential.prototype.create = function (nextPolicy, options) {
            return new AnonymousCredentialPolicy(nextPolicy, options);
        };
        return AnonymousCredential;
    }(Credential));

    /**
     * A ServiceURL represents a based URL class for ServiceURL, ContainerURL and etc.
     *
     * @export
     * @class StorageURL
     */
    var StorageURL = /** @class */ (function () {
        /**
         * Creates an instance of StorageURL.
         * @param {string} url
         * @param {Pipeline} pipeline
         * @memberof StorageURL
         */
        function StorageURL(url, pipeline) {
            // URL should be encoded and only once, protocol layer shouldn't encode URL again
            this.url = escapeURLPath(url);
            this.pipeline = pipeline;
            this.storageClientContext = new StorageClientContext(this.url, pipeline.toServiceClientOptions());
            this.isHttps = iEqual(getURLScheme(this.url) || "", "https");
            this.credential = new AnonymousCredential();
            for (var _i = 0, _a = this.pipeline.factories; _i < _a.length; _i++) {
                var factory = _a[_i];
                if (factory instanceof Credential) {
                    this.credential = factory;
                }
            }
            // Override protocol layer's default content-type
            var storageClientContext = this.storageClientContext;
            storageClientContext.requestContentType = undefined;
        }
        /**
         * A static method used to create a new Pipeline object with Credential provided.
         *
         * @static
         * @param {Credential} credential Such as AnonymousCredential, SharedKeyCredential or TokenCredential.
         * @param {INewPipelineOptions} [pipelineOptions] Optional. Options.
         * @returns {Pipeline} A new Pipeline object.
         * @memberof Pipeline
         */
        StorageURL.newPipeline = function (credential, pipelineOptions) {
            if (pipelineOptions === void 0) { pipelineOptions = {}; }
            // Order is important. Closer to the API at the top & closer to the network at the bottom.
            // The credential's policy factory must appear close to the wire so it can sign any
            // changes made by other factories (like UniqueRequestIDPolicyFactory)
            var factories = [
                new KeepAlivePolicyFactory(pipelineOptions.keepAliveOptions),
                new TelemetryPolicyFactory(pipelineOptions.telemetry),
                new UniqueRequestIDPolicyFactory(),
                new BrowserPolicyFactory(),
                deserializationPolicy(),
                new RetryPolicyFactory(pipelineOptions.retryOptions),
                new LoggingPolicyFactory(),
                credential
            ];
            return new Pipeline(factories, {
                HTTPClient: pipelineOptions.httpClient,
                logger: pipelineOptions.logger
            });
        };
        return StorageURL;
    }());

    /**
     * A BlobURL represents a URL to an Azure Storage blob; the blob may be a block blob,
     * append blob, or page blob.
     *
     * @export
     * @class BlobURL
     * @extends {StorageURL}
     */
    var BlobURL = /** @class */ (function (_super) {
        __extends(BlobURL, _super);
        /**
         * Creates an instance of BlobURL.
         * This method accepts an encoded URL or non-encoded URL pointing to a blob.
         * Encoded URL string will NOT be escaped twice, only special characters in URL path will be escaped.
         * If a blob name includes ? or %, blob name must be encoded in the URL.
         *
         * @param {string} url A URL string pointing to Azure Storage blob, such as
         *                     "https://myaccount.blob.core.windows.net/mycontainer/blob".
         *                     You can append a SAS if using AnonymousCredential, such as
         *                     "https://myaccount.blob.core.windows.net/mycontainer/blob?sasString".
         *                     This method accepts an encoded URL or non-encoded URL pointing to a blob.
         *                     Encoded URL string will NOT be escaped twice, only special characters in URL path will be escaped.
         *                     However, if a blob name includes ? or %, blob name must be encoded in the URL.
         *                     Such as a blob named "my?blob%", the URL should be "https://myaccount.blob.core.windows.net/mycontainer/my%3Fblob%25".
         * @param {Pipeline} pipeline Call StorageURL.newPipeline() to create a default
         *                            pipeline, or provide a customized pipeline.
         * @memberof BlobURL
         */
        function BlobURL(url, pipeline) {
            var _this = _super.call(this, url, pipeline) || this;
            _this.blobContext = new Blob$1(_this.storageClientContext);
            return _this;
        }
        /**
         * Creates a BlobURL object from an ContainerURL object.
         *
         * @static
         * @param {ContainerURL} containerURL A ContainerURL object
         * @param {string} blobName A blob name
         * @returns
         * @memberof BlobURL
         */
        BlobURL.fromContainerURL = function (containerURL, blobName) {
            return new BlobURL(appendToURLPath(containerURL.url, encodeURIComponent(blobName)), containerURL.pipeline);
        };
        /**
         * Creates a new BlobURL object identical to the source but with the
         * specified request policy pipeline.
         *
         * @param {Pipeline} pipeline
         * @returns {BlobURL}
         * @memberof BlobURL
         */
        BlobURL.prototype.withPipeline = function (pipeline) {
            return new BlobURL(this.url, pipeline);
        };
        /**
         * Creates a new BlobURL object identical to the source but with the specified snapshot timestamp.
         * Provide "" will remove the snapshot and return a URL to the base blob.
         *
         * @param {string} snapshot
         * @returns {BlobURL} A new BlobURL object identical to the source but with the specified snapshot timestamp
         * @memberof BlobURL
         */
        BlobURL.prototype.withSnapshot = function (snapshot) {
            return new BlobURL(setURLParameter(this.url, URLConstants.Parameters.SNAPSHOT, snapshot.length === 0 ? undefined : snapshot), this.pipeline);
        };
        /**
         * Reads or downloads a blob from the system, including its metadata and properties.
         * You can also call Get Blob to read a snapshot.
         *
         * * In Node.js, data returns in a Readable stream readableStreamBody
         * * In browsers, data returns in a promise blobBody
         *
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/get-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {number} offset From which position of the blob to download, >= 0
         * @param {number} [count] How much data to be downloaded, > 0. Will download to the end when undefined
         * @param {IBlobDownloadOptions} [options]
         * @returns {Promise<Models.BlobDownloadResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.download = function (aborter, offset, count, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                var res;
                var _this = this;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            options.blobAccessConditions = options.blobAccessConditions || {};
                            options.blobAccessConditions.modifiedAccessConditions =
                                options.blobAccessConditions.modifiedAccessConditions || {};
                            ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                            return [4 /*yield*/, this.blobContext.download({
                                    abortSignal: aborter,
                                    leaseAccessConditions: options.blobAccessConditions.leaseAccessConditions,
                                    modifiedAccessConditions: options.blobAccessConditions.modifiedAccessConditions,
                                    onDownloadProgress: isNode ? undefined : options.progress,
                                    range: offset === 0 && !count ? undefined : rangeToString({ offset: offset, count: count }),
                                    rangeGetContentMD5: options.rangeGetContentMD5,
                                    rangeGetContentCRC64: options.rangeGetContentCrc64,
                                    snapshot: options.snapshot,
                                    cpkInfo: options.customerProvidedKey
                                })];
                        case 1:
                            res = _a.sent();
                            // Return browser response immediately
                            if (!isNode) {
                                return [2 /*return*/, res];
                            }
                            // We support retrying when download stream unexpected ends in Node.js runtime
                            // Following code shouldn't be bundled into browser build, however some
                            // bundlers may try to bundle following code and "FileReadResponse.ts".
                            // In this case, "FileDownloadResponse.browser.ts" will be used as a shim of "FileDownloadResponse.ts"
                            // The config is in package.json "browser" field
                            if (options.maxRetryRequests === undefined || options.maxRetryRequests < 0) {
                                // TODO: Default value or make it a required parameter?
                                options.maxRetryRequests = DEFAULT_MAX_DOWNLOAD_RETRY_REQUESTS;
                            }
                            if (res.contentLength === undefined) {
                                throw new RangeError("File download response doesn't contain valid content length header");
                            }
                            if (!res.eTag) {
                                throw new RangeError("File download response doesn't contain valid etag header");
                            }
                            return [2 /*return*/, new BlobDownloadResponse(aborter, res, function (start) { return __awaiter(_this, void 0, void 0, function () {
                                    var updatedOptions;
                                    return __generator(this, function (_a) {
                                        switch (_a.label) {
                                            case 0:
                                                updatedOptions = {
                                                    leaseAccessConditions: options.blobAccessConditions.leaseAccessConditions,
                                                    modifiedAccessConditions: {
                                                        ifMatch: options.blobAccessConditions.modifiedAccessConditions.ifMatch || res.eTag,
                                                        ifModifiedSince: options.blobAccessConditions.modifiedAccessConditions
                                                            .ifModifiedSince,
                                                        ifNoneMatch: options.blobAccessConditions.modifiedAccessConditions.ifNoneMatch,
                                                        ifUnmodifiedSince: options.blobAccessConditions.modifiedAccessConditions
                                                            .ifUnmodifiedSince
                                                    },
                                                    range: rangeToString({
                                                        count: offset + res.contentLength - start,
                                                        offset: start
                                                    }),
                                                    rangeGetContentMD5: options.rangeGetContentMD5,
                                                    rangeGetContentCRC64: options.rangeGetContentCrc64,
                                                    snapshot: options.snapshot,
                                                    cpkInfo: options.customerProvidedKey
                                                };
                                                return [4 /*yield*/, this.blobContext.download(__assign({ abortSignal: aborter }, updatedOptions))];
                                            case 1: 
                                            // Debug purpose only
                                            // console.log(
                                            //   `Read from internal stream, range: ${
                                            //     updatedOptions.range
                                            //   }, options: ${JSON.stringify(updatedOptions)}`
                                            // );
                                            return [2 /*return*/, (_a.sent()).readableStreamBody];
                                        }
                                    });
                                }); }, offset, res.contentLength, {
                                    maxRetryRequests: options.maxRetryRequests,
                                    progress: options.progress
                                })];
                    }
                });
            });
        };
        /**
         * Returns all user-defined metadata, standard HTTP properties, and system properties
         * for the blob. It does not return the content of the blob.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/get-blob-properties
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {IBlobGetPropertiesOptions} [options]
         * @returns {Promise<Models.BlobGetPropertiesResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.getProperties = function (aborter, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.blobAccessConditions = options.blobAccessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.blobContext.getProperties({
                            abortSignal: aborter,
                            leaseAccessConditions: options.blobAccessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.blobAccessConditions.modifiedAccessConditions,
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * Marks the specified blob or snapshot for deletion. The blob is later deleted
         * during garbage collection. Note that in order to delete a blob, you must delete
         * all of its snapshots. You can delete both at the same time with the Delete
         * Blob operation.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/delete-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {IBlobDeleteOptions} [options]
         * @returns {Promise<Models.BlobDeleteResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.delete = function (aborter, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.blobAccessConditions = options.blobAccessConditions || {};
                    return [2 /*return*/, this.blobContext.deleteMethod({
                            abortSignal: aborter,
                            deleteSnapshots: options.deleteSnapshots,
                            leaseAccessConditions: options.blobAccessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.blobAccessConditions.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * Restores the contents and metadata of soft deleted blob and any associated
         * soft deleted snapshots. Undelete Blob is supported only on version 2017-07-29
         * or later.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/undelete-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @returns {Promise<Models.BlobUndeleteResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.undelete = function (aborter) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.blobContext.undelete({
                            abortSignal: aborter
                        })];
                });
            });
        };
        /**
         * Sets system properties on the blob.
         *
         * If no value provided, or no value provided for the specificed blob HTTP headers,
         * these blob HTTP headers without a value will be cleared.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/set-blob-properties
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {Models.BlobHTTPHeaders} [blobHTTPHeaders] If no value provided, or no value provided for
         *                                                   the specificed blob HTTP headers, these blob HTTP
         *                                                   headers without a value will be cleared.
         * @param {IBlobSetHTTPHeadersOptions} [options]
         * @returns {Promise<Models.BlobSetHTTPHeadersResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.setHTTPHeaders = function (aborter, blobHTTPHeaders, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.blobAccessConditions = options.blobAccessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.blobContext.setHTTPHeaders({
                            abortSignal: aborter,
                            blobHTTPHeaders: blobHTTPHeaders,
                            leaseAccessConditions: options.blobAccessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.blobAccessConditions.modifiedAccessConditions,
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * Sets user-defined metadata for the specified blob as one or more name-value pairs.
         *
         * If no option provided, or no metadata defined in the parameter, the blob
         * metadata will be removed.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/set-blob-metadata
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {IMetadata} [metadata] Replace existing metadata with this value.
         *                               If no value provided the existing metadata will be removed.
         * @param {IBlobSetMetadataOptions} [options]
         * @returns {Promise<Models.BlobSetMetadataResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.setMetadata = function (aborter, metadata, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.blobAccessConditions = options.blobAccessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.blobContext.setMetadata({
                            abortSignal: aborter,
                            leaseAccessConditions: options.blobAccessConditions.leaseAccessConditions,
                            metadata: metadata,
                            modifiedAccessConditions: options.blobAccessConditions.modifiedAccessConditions,
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * Establishes and manages a lock on a blob for write and delete operations.
         * The lock duration can be 15 to 60 seconds, or can be infinite.
         * In versions prior to 2012-02-12, the lock duration is 60 seconds.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/lease-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} proposedLeaseId Can be specified in any valid GUID string format
         * @param {number} duration The lock duration can be 15 to 60 seconds, or can be infinite
         * @param {IBlobAcquireLeaseOptions} [options]
         * @returns {Promise<Models.BlobAcquireLeaseResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.acquireLease = function (aborter, proposedLeaseId, duration, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.blobContext.acquireLease({
                            abortSignal: aborter,
                            duration: duration,
                            modifiedAccessConditions: options.modifiedAccessConditions,
                            proposedLeaseId: proposedLeaseId
                        })];
                });
            });
        };
        /**
         * To free the lease if it is no longer needed so that another client may immediately
         * acquire a lease against the blob.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/lease-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} leaseId
         * @param {IBlobReleaseLeaseOptions} [options]
         * @returns {Promise<Models.BlobReleaseLeaseResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.releaseLease = function (aborter, leaseId, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.blobContext.releaseLease(leaseId, {
                            abortSignal: aborter,
                            modifiedAccessConditions: options.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * To renew an existing lease.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/lease-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} leaseId
         * @param {IBlobRenewLeaseOptions} [options]
         * @returns {Promise<Models.BlobRenewLeaseResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.renewLease = function (aborter, leaseId, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.blobContext.renewLease(leaseId, {
                            abortSignal: aborter,
                            modifiedAccessConditions: options.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * To change the ID of an existing lease.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/lease-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} leaseId
         * @param {string} proposedLeaseId
         * @param {IBlobChangeLeaseOptions} [options]
         * @returns {Promise<Models.BlobChangeLeaseResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.changeLease = function (aborter, leaseId, proposedLeaseId, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.blobContext.changeLease(leaseId, proposedLeaseId, {
                            abortSignal: aborter,
                            modifiedAccessConditions: options.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * To end the lease but ensure that another client cannot acquire a new lease
         * until the current lease period has expired.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/lease-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {number} [breakPeriod]
         * @param {IBlobBreakLeaseOptions} [options]
         * @returns {Promise<Models.BlobBreakLeaseResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.breakLease = function (aborter, breakPeriod, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.blobContext.breakLease({
                            abortSignal: aborter,
                            breakPeriod: breakPeriod,
                            modifiedAccessConditions: options.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * Creates a read-only snapshot of a blob.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/snapshot-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {IBlobCreateSnapshotOptions} [options]
         * @returns {Promise<Models.BlobCreateSnapshotResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.createSnapshot = function (aborter, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.blobAccessConditions = options.blobAccessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.blobContext.createSnapshot({
                            abortSignal: aborter,
                            leaseAccessConditions: options.blobAccessConditions.leaseAccessConditions,
                            metadata: options.metadata,
                            modifiedAccessConditions: options.blobAccessConditions.modifiedAccessConditions,
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * Asynchronously copies a blob to a destination within the storage account.
         * In version 2012-02-12 and later, the source for a Copy Blob operation can be
         * a committed blob in any Azure storage account.
         * Beginning with version 2015-02-21, the source for a Copy Blob operation can be
         * an Azure file in any Azure storage account.
         * Only storage accounts created on or after June 7th, 2012 allow the Copy Blob
         * operation to copy from another storage account.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/copy-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} copySource
         * @param {IBlobStartCopyFromURLOptions} [options]
         * @returns {Promise<Models.BlobStartCopyFromURLResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.startCopyFromURL = function (aborter, copySource, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.blobAccessConditions = options.blobAccessConditions || {};
                    options.sourceModifiedAccessConditions = options.sourceModifiedAccessConditions || {};
                    return [2 /*return*/, this.blobContext.startCopyFromURL(copySource, {
                            abortSignal: aborter,
                            leaseAccessConditions: options.blobAccessConditions.leaseAccessConditions,
                            metadata: options.metadata,
                            modifiedAccessConditions: options.blobAccessConditions.modifiedAccessConditions,
                            sourceModifiedAccessConditions: {
                                sourceIfMatch: options.sourceModifiedAccessConditions.ifMatch,
                                sourceIfModifiedSince: options.sourceModifiedAccessConditions.ifModifiedSince,
                                sourceIfNoneMatch: options.sourceModifiedAccessConditions.ifNoneMatch,
                                sourceIfUnmodifiedSince: options.sourceModifiedAccessConditions.ifUnmodifiedSince
                            },
                            rehydratePriority: options.rehydratePriority,
                            tier: toAccessTier(options.tier)
                        })];
                });
            });
        };
        /**
         * Aborts a pending asynchronous Copy Blob operation, and leaves a destination blob with zero
         * length and full metadata. Version 2012-02-12 and newer.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/abort-copy-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} copyId
         * @param {IBlobAbortCopyFromURLOptions} [options]
         * @returns {Promise<Models.BlobAbortCopyFromURLResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.abortCopyFromURL = function (aborter, copyId, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.blobContext.abortCopyFromURL(copyId, {
                            abortSignal: aborter,
                            leaseAccessConditions: options.leaseAccessConditions
                        })];
                });
            });
        };
        /**
         * The synchronous Copy From URL operation copies a blob or an internet resource to a new blob. It will not
         * return a response until the copy is complete.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/copy-blob-from-url
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} copySource The source URL to copy from, Shared Access Signature(SAS) maybe needed for authentication
         * @param {IBlobSyncCopyFromURLOptions} [options={}]
         * @returns {Promise<Models.BlobCopyFromURLResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.syncCopyFromURL = function (aborter, copySource, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.blobAccessConditions = options.blobAccessConditions || {};
                    options.sourceModifiedAccessConditions = options.sourceModifiedAccessConditions || {};
                    return [2 /*return*/, this.blobContext.copyFromURL(copySource, {
                            abortSignal: aborter,
                            metadata: options.metadata,
                            leaseAccessConditions: options.blobAccessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.blobAccessConditions.modifiedAccessConditions,
                            sourceModifiedAccessConditions: {
                                sourceIfMatch: options.sourceModifiedAccessConditions.ifMatch,
                                sourceIfModifiedSince: options.sourceModifiedAccessConditions.ifModifiedSince,
                                sourceIfNoneMatch: options.sourceModifiedAccessConditions.ifNoneMatch,
                                sourceIfUnmodifiedSince: options.sourceModifiedAccessConditions.ifUnmodifiedSince
                            }
                        })];
                });
            });
        };
        /**
         * Sets the tier on a blob. The operation is allowed on a page blob in a premium
         * storage account and on a block blob in a blob storage account (locally redundant
         * storage only). A premium page blob's tier determines the allowed size, IOPS,
         * and bandwidth of the blob. A block blob's tier determines Hot/Cool/Archive
         * storage type. This operation does not update the blob's ETag.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/set-blob-tier
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {BlockBlobTier | PremiumPageBlobTier | string} tier
         * @param {IBlobSetTierOptions} [options]
         * @returns {Promise<Models.BlobsSetTierResponse>}
         * @memberof BlobURL
         */
        BlobURL.prototype.setTier = function (aborter, tier, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.blobContext.setTier(toAccessTier(tier), {
                                abortSignal: aborter,
                                leaseAccessConditions: options.leaseAccessConditions,
                                rehydratePriority: options.rehydratePriority
                            })];
                        case 1: return [2 /*return*/, _a.sent()];
                    }
                });
            });
        };
        return BlobURL;
    }(StorageURL));

    /**
     * AppendBlobURL defines a set of operations applicable to append blobs.
     *
     * @export
     * @class AppendBlobURL
     * @extends {StorageURL}
     */
    var AppendBlobURL = /** @class */ (function (_super) {
        __extends(AppendBlobURL, _super);
        /**
         * Creates an instance of AppendBlobURL.
         * This method accepts an encoded URL or non-encoded URL pointing to an append blob.
         * Encoded URL string will NOT be escaped twice, only special characters in URL path will be escaped.
         * If a blob name includes ? or %, blob name must be encoded in the URL.
         *
         * @param {string} url A URL string pointing to Azure Storage append blob, such as
         *                     "https://myaccount.blob.core.windows.net/mycontainer/appendblob". You can
         *                     append a SAS if using AnonymousCredential, such as
         *                     "https://myaccount.blob.core.windows.net/mycontainer/appendblob?sasString".
         *                     This method accepts an encoded URL or non-encoded URL pointing to a blob.
         *                     Encoded URL string will NOT be escaped twice, only special characters in URL path will be escaped.
         *                     However, if a blob name includes ? or %, blob name must be encoded in the URL.
         *                     Such as a blob named "my?blob%", the URL should be "https://myaccount.blob.core.windows.net/mycontainer/my%3Fblob%25".
         * @param {Pipeline} pipeline Call StorageURL.newPipeline() to create a default
         *                            pipeline, or provide a customized pipeline.
         * @memberof AppendBlobURL
         */
        function AppendBlobURL(url, pipeline) {
            var _this = _super.call(this, url, pipeline) || this;
            _this.appendBlobContext = new AppendBlob(_this.storageClientContext);
            return _this;
        }
        /**
         * Creates a AppendBlobURL object from ContainerURL instance.
         *
         * @static
         * @param {ContainerURL} containerURL A ContainerURL object
         * @param {string} blobName An append blob name
         * @returns {AppendBlobURL}
         * @memberof AppendBlobURL
         */
        AppendBlobURL.fromContainerURL = function (containerURL, blobName) {
            return new AppendBlobURL(appendToURLPath(containerURL.url, encodeURIComponent(blobName)), containerURL.pipeline);
        };
        /**
         * Creates a AppendBlobURL object from BlobURL instance.
         *
         * @static
         * @param {BlobURL} blobURL
         * @returns {AppendBlobURL}
         * @memberof AppendBlobURL
         */
        AppendBlobURL.fromBlobURL = function (blobURL) {
            return new AppendBlobURL(blobURL.url, blobURL.pipeline);
        };
        /**
         * Creates a new AppendBlobURL object identical to the source but with the
         * specified request policy pipeline.
         *
         * @param {Pipeline} pipeline
         * @returns {AppendBlobURL}
         * @memberof AppendBlobURL
         */
        AppendBlobURL.prototype.withPipeline = function (pipeline) {
            return new AppendBlobURL(this.url, pipeline);
        };
        /**
         * Creates a new AppendBlobURL object identical to the source but with the
         * specified snapshot timestamp.
         * Provide "" will remove the snapshot and return a URL to the base blob.
         *
         * @param {string} snapshot
         * @returns {AppendBlobURL}
         * @memberof AppendBlobURL
         */
        AppendBlobURL.prototype.withSnapshot = function (snapshot) {
            return new AppendBlobURL(setURLParameter(this.url, URLConstants.Parameters.SNAPSHOT, snapshot.length === 0 ? undefined : snapshot), this.pipeline);
        };
        /**
         * Creates a 0-length append blob. Call AppendBlock to append data to an append blob.
         * @see https://docs.microsoft.com/rest/api/storageservices/put-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {IAppendBlobCreateOptions} [options]
         * @returns {Promise<Models.AppendBlobsCreateResponse>}
         * @memberof AppendBlobURL
         */
        AppendBlobURL.prototype.create = function (aborter, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.appendBlobContext.create(0, {
                            abortSignal: aborter,
                            blobHTTPHeaders: options.blobHTTPHeaders,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            metadata: options.metadata,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * Commits a new block of data to the end of the existing append blob.
         * @see https://docs.microsoft.com/rest/api/storageservices/append-block
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {HttpRequestBody} body
         * @param {number} contentLength Length of the body in bytes
         * @param {IAppendBlobAppendBlockOptions} [options]
         * @returns {Promise<Models.AppendBlobsAppendBlockResponse>}
         * @memberof AppendBlobURL
         */
        AppendBlobURL.prototype.appendBlock = function (aborter, body, contentLength, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.appendBlobContext.appendBlock(body, contentLength, {
                            abortSignal: aborter,
                            appendPositionAccessConditions: options.accessConditions.appendPositionAccessConditions,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            onUploadProgress: options.progress,
                            transactionalContentMD5: options.transactionalContentMD5,
                            transactionalContentCrc64: options.transactionalContentCrc64,
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * The Append Block operation commits a new block of data to the end of an existing append blob
         * where the contents are read from a source url.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/append-block-from-url
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} sourceURL
         *                 The url to the blob that will be the source of the copy. A source blob in the same storage account can
         *                 be authenticated via Shared Key. However, if the source is a blob in another account, the source blob
         *                 must either be public or must be authenticated via a shared access signature. If the source blob is
         *                 public, no authentication is required to perform the operation.
         * @param {number} sourceOffset Offset in source to be appended
         * @param {number} count Number of bytes to be appended as a block
         * @param {IAppendBlobAppendBlockFromURLOptions} [options={}]
         * @returns {Promise<Models.AppendBlobAppendBlockFromUrlResponse>}
         * @memberof AppendBlobURL
         */
        AppendBlobURL.prototype.appendBlockFromURL = function (aborter, sourceURL, sourceOffset, count, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    options.sourceModifiedAccessConditions = options.sourceModifiedAccessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.appendBlobContext.appendBlockFromUrl(sourceURL, 0, {
                            abortSignal: aborter,
                            sourceRange: rangeToString({ offset: sourceOffset, count: count }),
                            sourceContentMD5: options.sourceContentMD5,
                            sourceContentCrc64: options.sourceContentCrc64,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            appendPositionAccessConditions: options.accessConditions.appendPositionAccessConditions,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            sourceModifiedAccessConditions: {
                                sourceIfMatch: options.sourceModifiedAccessConditions.ifMatch,
                                sourceIfModifiedSince: options.sourceModifiedAccessConditions.ifModifiedSince,
                                sourceIfNoneMatch: options.sourceModifiedAccessConditions.ifNoneMatch,
                                sourceIfUnmodifiedSince: options.sourceModifiedAccessConditions.ifUnmodifiedSince
                            },
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        return AppendBlobURL;
    }(BlobURL));

    var MutexLockStatus;
    (function (MutexLockStatus) {
        MutexLockStatus[MutexLockStatus["LOCKED"] = 0] = "LOCKED";
        MutexLockStatus[MutexLockStatus["UNLOCKED"] = 1] = "UNLOCKED";
    })(MutexLockStatus || (MutexLockStatus = {}));
    /**
     * An async mutex lock.
     *
     * @export
     * @class Mutex
     */
    var Mutex = /** @class */ (function () {
        function Mutex() {
        }
        /**
         * Lock for a specific key. If the lock has been acquired by another customer, then
         * will wait until getting the lock.
         *
         * @static
         * @param {string} key lock key
         * @returns {Promise<void>}
         * @memberof Mutex
         */
        Mutex.lock = function (key) {
            return __awaiter(this, void 0, void 0, function () {
                var _this = this;
                return __generator(this, function (_a) {
                    return [2 /*return*/, new Promise(function (resolve) {
                            if (_this.keys[key] === undefined || _this.keys[key] === MutexLockStatus.UNLOCKED) {
                                _this.keys[key] = MutexLockStatus.LOCKED;
                                resolve();
                            }
                            else {
                                _this.onUnlockEvent(key, function () {
                                    _this.keys[key] = MutexLockStatus.LOCKED;
                                    resolve();
                                });
                            }
                        })];
                });
            });
        };
        /**
         * Unlock a key.
         *
         * @static
         * @param {string} key
         * @returns {Promise<void>}
         * @memberof Mutex
         */
        Mutex.unlock = function (key) {
            return __awaiter(this, void 0, void 0, function () {
                var _this = this;
                return __generator(this, function (_a) {
                    return [2 /*return*/, new Promise(function (resolve) {
                            if (_this.keys[key] === MutexLockStatus.LOCKED) {
                                _this.emitUnlockEvent(key);
                            }
                            delete _this.keys[key];
                            resolve();
                        })];
                });
            });
        };
        Mutex.onUnlockEvent = function (key, handler) {
            if (this.listeners[key] === undefined) {
                this.listeners[key] = [handler];
            }
            else {
                this.listeners[key].push(handler);
            }
        };
        Mutex.emitUnlockEvent = function (key) {
            var _this = this;
            if (this.listeners[key] !== undefined && this.listeners[key].length > 0) {
                var handler_1 = this.listeners[key].shift();
                setImmediate(function () {
                    handler_1.call(_this);
                });
            }
        };
        Mutex.keys = {};
        Mutex.listeners = {};
        return Mutex;
    }());

    /**
     * A BatchRequest represents a based class for BatchDeleteRequest and BatchSetTierRequest.
     *
     * @export
     * @class BatchRequest
     */
    var BatchRequest = /** @class */ (function () {
        function BatchRequest() {
            this.batch = "batch";
            this.batchRequest = new InnerBatchRequest();
        }
        /**
         * Get the value of Content-Type for a batch request.
         * The value must be multipart/mixed with a batch boundary.
         * Example: multipart/mixed; boundary=batch_a81786c8-e301-4e42-a729-a32ca24ae252
         */
        BatchRequest.prototype.getMultiPartContentType = function () {
            return this.batchRequest.getMultipartContentType();
        };
        /**
         * Get assembled HTTP request body for sub requests.
         */
        BatchRequest.prototype.getHttpRequestBody = function () {
            return this.batchRequest.getHttpRequestBody();
        };
        /**
         * Get sub requests that are added into the batch request.
         */
        BatchRequest.prototype.getSubRequests = function () {
            return this.batchRequest.getSubRequests();
        };
        BatchRequest.prototype.addSubRequestInternal = function (subRequest, assembleSubRequestFunc) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, Mutex.lock(this.batch)];
                        case 1:
                            _a.sent();
                            _a.label = 2;
                        case 2:
                            _a.trys.push([2, , 4, 6]);
                            this.batchRequest.preAddSubRequest(subRequest);
                            return [4 /*yield*/, assembleSubRequestFunc()];
                        case 3:
                            _a.sent();
                            this.batchRequest.postAddSubRequest(subRequest);
                            return [3 /*break*/, 6];
                        case 4: return [4 /*yield*/, Mutex.unlock(this.batch)];
                        case 5:
                            _a.sent();
                            return [7 /*endfinally*/];
                        case 6: return [2 /*return*/];
                    }
                });
            });
        };
        return BatchRequest;
    }());
    /**
     * A BatchDeleteRequest represents a batch delete request, which consists of one or more delete operations.
     *
     * @export
     * @class BatchDeleteRequest
     * @extends {BatchRequest}
     */
    var BatchDeleteRequest = /** @class */ (function (_super) {
        __extends(BatchDeleteRequest, _super);
        function BatchDeleteRequest() {
            return _super.call(this) || this;
        }
        BatchDeleteRequest.prototype.addSubRequest = function (urlOrBlobURL, credentialOrOptions, options) {
            return __awaiter(this, void 0, void 0, function () {
                var url, credential;
                var _this = this;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (typeof urlOrBlobURL === 'string' && credentialOrOptions instanceof Credential) {
                                // First overload
                                url = urlOrBlobURL;
                                credential = credentialOrOptions;
                            }
                            else if (urlOrBlobURL instanceof BlobURL) {
                                // Second overload
                                url = urlOrBlobURL.url;
                                credential = urlOrBlobURL.credential;
                                options = credentialOrOptions;
                            }
                            else {
                                throw new RangeError("Invalid arguments. Either url and credential, or BlobURL need be provided.");
                            }
                            if (!options) {
                                options = {};
                            }
                            return [4 /*yield*/, _super.prototype.addSubRequestInternal.call(this, {
                                    url: url,
                                    credential: credential
                                }, function () { return __awaiter(_this, void 0, void 0, function () {
                                    return __generator(this, function (_a) {
                                        switch (_a.label) {
                                            case 0: return [4 /*yield*/, new BlobURL(url, this.batchRequest.createPipeline(credential)).delete(Aborter.none, options)];
                                            case 1:
                                                _a.sent();
                                                return [2 /*return*/];
                                        }
                                    });
                                }); })];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        return BatchDeleteRequest;
    }(BatchRequest));
    /**
     * A BatchSetTierRequest represents a batch set tier request, which consists of one or more set tier operations.
     *
     * @export
     * @class BatchSetTierRequest
     * @extends {BatchRequest}
     */
    var BatchSetTierRequest = /** @class */ (function (_super) {
        __extends(BatchSetTierRequest, _super);
        function BatchSetTierRequest() {
            return _super.call(this) || this;
        }
        BatchSetTierRequest.prototype.addSubRequest = function (urlOrBlobURL, credentialOrTier, tierOrOptions, options) {
            return __awaiter(this, void 0, void 0, function () {
                var url, credential, tier;
                var _this = this;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (typeof urlOrBlobURL === 'string' && credentialOrTier instanceof Credential) {
                                // First overload
                                url = urlOrBlobURL;
                                credential = credentialOrTier;
                                tier = tierOrOptions;
                            }
                            else if (urlOrBlobURL instanceof BlobURL) {
                                // Second overload
                                url = urlOrBlobURL.url;
                                credential = urlOrBlobURL.credential;
                                tier = credentialOrTier;
                                options = tierOrOptions;
                            }
                            else {
                                throw new RangeError("Invalid arguments. Either url and credential, or BlobURL need be provided.");
                            }
                            if (!options) {
                                options = {};
                            }
                            return [4 /*yield*/, _super.prototype.addSubRequestInternal.call(this, {
                                    url: url,
                                    credential: credential
                                }, function () { return __awaiter(_this, void 0, void 0, function () {
                                    return __generator(this, function (_a) {
                                        switch (_a.label) {
                                            case 0: return [4 /*yield*/, new BlobURL(url, this.batchRequest.createPipeline(credential)).setTier(Aborter.none, tier, options)];
                                            case 1:
                                                _a.sent();
                                                return [2 /*return*/];
                                        }
                                    });
                                }); })];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        return BatchSetTierRequest;
    }(BatchRequest));
    /**
     * Inner batch request class which is responsible for assembling and serializing sub requests.
     * See https://docs.microsoft.com/en-us/rest/api/storageservices/blob-batch#request-body for how request get assembled.
     */
    var InnerBatchRequest = /** @class */ (function () {
        function InnerBatchRequest() {
            this.operationCount = 0;
            this.body = "";
            var tempGuid = generateUuid();
            // batch_{batchid}
            this.boundary = "batch_" + tempGuid;
            // --batch_{batchid}
            // Content-Type: application/http
            // Content-Transfer-Encoding: binary
            this.subRequestPrefix = "--" + this.boundary + HTTP_LINE_ENDING + HeaderConstants.CONTENT_TYPE + ": application/http" + HTTP_LINE_ENDING + HeaderConstants.CONTENT_TRANSFER_ENCODING + ": binary";
            // multipart/mixed; boundary=batch_{batchid}
            this.multipartContentType = "multipart/mixed; boundary=" + this.boundary;
            // --batch_{batchid}--
            this.batchRequestEnding = "--" + this.boundary + "--";
            this.subRequests = new Map();
        }
        /**
         * Create pipeline to assemble sub requests. The idea here is to use exising
         * credential and serialization/deserialization components, with additional policies to
         * filter unnecessary headers, assemble sub requests into request's body
         * and intercept request from going to wire.
         * @param credential
         */
        InnerBatchRequest.prototype.createPipeline = function (credential) {
            var isAnonymousCreds = credential instanceof AnonymousCredential;
            var policyFactoryLength = 3 + (isAnonymousCreds ? 0 : 1); // [deserilizationPolicy, BatchHeaderFilterPolicyFactory, (Optional)Credential, BatchRequestAssemblePolicyFactory]
            var factories = new Array(policyFactoryLength);
            factories[0] = deserializationPolicy(); // Default deserializationPolicy is provided by protocol layer
            factories[1] = new BatchHeaderFilterPolicyFactory(); // Use batch header filter policy to exclude unnecessary headers
            if (!isAnonymousCreds) {
                factories[2] = credential;
            }
            factories[policyFactoryLength - 1] = new BatchRequestAssemblePolicyFactory(this); // Use batch assemble policy to assemble request and intercept request from going to wire
            return new Pipeline(factories, {});
        };
        InnerBatchRequest.prototype.appendSubRequestToBody = function (request) {
            // Start to assemble sub request
            this.body +=
                [
                    this.subRequestPrefix,
                    HeaderConstants.CONTENT_ID + ": " + this.operationCount,
                    "",
                    request.method.toString() + " " + getURLPathAndQuery(request.url) + " " + HTTP_VERSION_1_1 + HTTP_LINE_ENDING // sub request start line with method
                ].join(HTTP_LINE_ENDING);
            for (var _i = 0, _a = request.headers.headersArray(); _i < _a.length; _i++) {
                var header = _a[_i];
                this.body += header.name + ": " + header.value + HTTP_LINE_ENDING;
            }
            this.body += HTTP_LINE_ENDING; // sub request's headers need be ending with an empty line
            // No body to assemble for current batch request support
            // End to assemble sub request
        };
        InnerBatchRequest.prototype.preAddSubRequest = function (subRequest) {
            if (this.operationCount >= BATCH_MAX_REQUEST) {
                throw new RangeError("Cannot exceed " + BATCH_MAX_REQUEST + " sub requests in a single batch");
            }
            // Fast fail if url for sub request is invalid
            var path = getURLPath(subRequest.url);
            if (!path || path == "") {
                throw new RangeError("Invalid url for sub request: '" + subRequest.url + "'");
            }
        };
        InnerBatchRequest.prototype.postAddSubRequest = function (subRequest) {
            this.subRequests.set(this.operationCount, subRequest);
            this.operationCount++;
        };
        // Return the http request body with assembling the ending line to the sub request body.
        InnerBatchRequest.prototype.getHttpRequestBody = function () {
            return "" + this.body + this.batchRequestEnding + HTTP_LINE_ENDING;
        };
        InnerBatchRequest.prototype.getMultipartContentType = function () {
            return this.multipartContentType;
        };
        InnerBatchRequest.prototype.getSubRequests = function () {
            return this.subRequests;
        };
        return InnerBatchRequest;
    }());
    var BatchRequestAssemblePolicy = /** @class */ (function (_super) {
        __extends(BatchRequestAssemblePolicy, _super);
        function BatchRequestAssemblePolicy(batchRequest, nextPolicy, options) {
            var _this = _super.call(this, nextPolicy, options) || this;
            _this.dummyResponse = {
                request: new WebResource(),
                status: 200,
                headers: new HttpHeaders()
            };
            _this.batchRequest = batchRequest;
            return _this;
        }
        BatchRequestAssemblePolicy.prototype.sendRequest = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.batchRequest.appendSubRequestToBody(request)];
                        case 1:
                            _a.sent();
                            return [2 /*return*/, this.dummyResponse]; // Intercept request from going to wire
                    }
                });
            });
        };
        return BatchRequestAssemblePolicy;
    }(BaseRequestPolicy));
    var BatchRequestAssemblePolicyFactory = /** @class */ (function () {
        function BatchRequestAssemblePolicyFactory(batchRequest) {
            this.batchRequest = batchRequest;
        }
        BatchRequestAssemblePolicyFactory.prototype.create = function (nextPolicy, options) {
            return new BatchRequestAssemblePolicy(this.batchRequest, nextPolicy, options);
        };
        return BatchRequestAssemblePolicyFactory;
    }());
    var BatchHeaderFilterPolicy = /** @class */ (function (_super) {
        __extends(BatchHeaderFilterPolicy, _super);
        function BatchHeaderFilterPolicy(nextPolicy, options) {
            return _super.call(this, nextPolicy, options) || this;
        }
        BatchHeaderFilterPolicy.prototype.sendRequest = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                var xMsHeaderName, _i, _a, header;
                return __generator(this, function (_b) {
                    xMsHeaderName = "";
                    for (_i = 0, _a = request.headers.headersArray(); _i < _a.length; _i++) {
                        header = _a[_i];
                        if (iEqual(header.name, HeaderConstants.X_MS_VERSION)) {
                            xMsHeaderName = header.name;
                        }
                    }
                    if (xMsHeaderName !== "") {
                        request.headers.remove(xMsHeaderName); // The subrequests should not have the x-ms-version header.
                    }
                    return [2 /*return*/, this._nextPolicy.sendRequest(request)];
                });
            });
        };
        return BatchHeaderFilterPolicy;
    }(BaseRequestPolicy));
    var BatchHeaderFilterPolicyFactory = /** @class */ (function () {
        function BatchHeaderFilterPolicyFactory() {
        }
        BatchHeaderFilterPolicyFactory.prototype.create = function (nextPolicy, options) {
            return new BatchHeaderFilterPolicy(nextPolicy, options);
        };
        return BatchHeaderFilterPolicyFactory;
    }());

    /**
     * BlockBlobURL defines a set of operations applicable to block blobs.
     *
     * @export
     * @class BlockBlobURL
     * @extends {StorageURL}
     */
    var BlockBlobURL = /** @class */ (function (_super) {
        __extends(BlockBlobURL, _super);
        /**
         * Creates an instance of BlockBlobURL.
         * This method accepts an encoded URL or non-encoded URL pointing to a block blob.
         * Encoded URL string will NOT be escaped twice, only special characters in URL path will be escaped.
         * If a blob name includes ? or %, blob name must be encoded in the URL.
         *
         * @param {string} url A URL string pointing to Azure Storage block blob, such as
         *                     "https://myaccount.blob.core.windows.net/mycontainer/blockblob". You can
         *                     append a SAS if using AnonymousCredential, such as
         *                     "https://myaccount.blob.core.windows.net/mycontainer/blockblob?sasString".
         *                     This method accepts an encoded URL or non-encoded URL pointing to a blob.
         *                     Encoded URL string will NOT be escaped twice, only special characters in URL path will be escaped.
         *                     However, if a blob name includes ? or %, blob name must be encoded in the URL.
         *                     Such as a blob named "my?blob%", the URL should be "https://myaccount.blob.core.windows.net/mycontainer/my%3Fblob%25".
         * @param {Pipeline} pipeline Call StorageURL.newPipeline() to create a default
         *                            pipeline, or provide a customized pipeline.
         * @memberof BlockBlobURL
         */
        function BlockBlobURL(url, pipeline) {
            var _this = _super.call(this, url, pipeline) || this;
            _this.blockBlobContext = new BlockBlob(_this.storageClientContext);
            return _this;
        }
        /**
         * Creates a BlockBlobURL object from ContainerURL instance.
         *
         * @static
         * @param {ContainerURL} containerURL A ContainerURL object
         * @param {string} blobName A block blob name
         * @returns {BlockBlobURL}
         * @memberof BlockBlobURL
         */
        BlockBlobURL.fromContainerURL = function (containerURL, blobName) {
            return new BlockBlobURL(appendToURLPath(containerURL.url, encodeURIComponent(blobName)), containerURL.pipeline);
        };
        /**
         * Creates a BlockBlobURL object from BlobURL instance.
         *
         * @static
         * @param {BlobURL} blobURL
         * @returns {BlockBlobURL}
         * @memberof BlockBlobURL
         */
        BlockBlobURL.fromBlobURL = function (blobURL) {
            return new BlockBlobURL(blobURL.url, blobURL.pipeline);
        };
        /**
         * Creates a new BlockBlobURL object identical to the source but with the
         * specified request policy pipeline.
         *
         * @param {Pipeline} pipeline
         * @returns {BlockBlobURL}
         * @memberof BlockBlobURL
         */
        BlockBlobURL.prototype.withPipeline = function (pipeline) {
            return new BlockBlobURL(this.url, pipeline);
        };
        /**
         * Creates a new BlockBlobURL object identical to the source but with the
         * specified snapshot timestamp.
         * Provide "" will remove the snapshot and return a URL to the base blob.
         *
         * @param {string} snapshot
         * @returns {BlockBlobURL}
         * @memberof BlockBlobURL
         */
        BlockBlobURL.prototype.withSnapshot = function (snapshot) {
            return new BlockBlobURL(setURLParameter(this.url, URLConstants.Parameters.SNAPSHOT, snapshot.length === 0 ? undefined : snapshot), this.pipeline);
        };
        /**
         * Creates a new block blob, or updates the content of an existing block blob.
         * Updating an existing block blob overwrites any existing metadata on the blob.
         * Partial updates are not supported; the content of the existing blob is
         * overwritten with the new content. To perform a partial update of a block blob's,
         * use stageBlock and commitBlockList.
         *
         * This is a non-parallel uploading method, please use uploadFileToBlockBlob(),
         * uploadStreamToBlockBlob() or uploadBrowserDataToBlockBlob() for better performance
         * with concurrency uploading.
         *
         * @see https://docs.microsoft.com/rest/api/storageservices/put-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {HttpRequestBody} body Blob, string, ArrayBuffer, ArrayBufferView or a function
         *                               which returns a new Readable stream whose offset is from data source beginning.
         * @param {number} contentLength Length of body in bytes. Use Buffer.byteLength() to calculate body length for a
         *                               string including non non-Base64/Hex-encoded characters.
         * @param {IBlockBlobUploadOptions} [options]
         * @returns {Promise<Models.BlockBlobUploadResponse>}
         * @memberof BlockBlobURL
         */
        BlockBlobURL.prototype.upload = function (aborter, body, contentLength, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.blockBlobContext.upload(body, contentLength, {
                            abortSignal: aborter,
                            blobHTTPHeaders: options.blobHTTPHeaders,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            metadata: options.metadata,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            onUploadProgress: options.progress,
                            cpkInfo: options.customerProvidedKey,
                            tier: toAccessTier(options.tier)
                        })];
                });
            });
        };
        /**
         * Uploads the specified block to the block blob's "staging area" to be later
         * committed by a call to commitBlockList.
         * @see https://docs.microsoft.com/rest/api/storageservices/put-block
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} blockId A 64-byte value that is base64-encoded
         * @param {HttpRequestBody} body
         * @param {number} contentLength
         * @param {IBlockBlobStageBlockOptions} [options]
         * @returns {Promise<Models.BlockBlobStageBlockResponse>}
         * @memberof BlockBlobURL
         */
        BlockBlobURL.prototype.stageBlock = function (aborter, blockId, body, contentLength, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.blockBlobContext.stageBlock(blockId, contentLength, body, {
                            abortSignal: aborter,
                            leaseAccessConditions: options.leaseAccessConditions,
                            onUploadProgress: options.progress,
                            transactionalContentMD5: options.transactionalContentMD5,
                            transactionalContentCrc64: options.transactionalContentCrc64,
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * The Stage Block From URL operation creates a new block to be committed as part
         * of a blob where the contents are read from a URL.
         * This API is available starting in version 2018-03-28.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/put-block-from-url
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} blockId A 64-byte value that is base64-encoded
         * @param {string} sourceURL Specifies the URL of the blob. The value
         *                           may be a URL of up to 2 KB in length that specifies a blob.
         *                           The value should be URL-encoded as it would appear
         *                           in a request URI. The source blob must either be public
         *                           or must be authenticated via a shared access signature.
         *                           If the source blob is public, no authentication is required
         *                           to perform the operation. Here are some examples of source object URLs:
         *                           - https://myaccount.blob.core.windows.net/mycontainer/myblob
         *                           - https://myaccount.blob.core.windows.net/mycontainer/myblob?snapshot=<DateTime>
         * @param {number} offset From which position of the blob to download, >= 0
         * @param {number} [count] How much data to be downloaded, > 0. Will download to the end when undefined
         * @param {IBlockBlobStageBlockFromURLOptions} [options={}]
         * @returns {Promise<Models.BlockBlobStageBlockFromURLResponse>}
         * @memberof BlockBlobURL
         */
        BlockBlobURL.prototype.stageBlockFromURL = function (aborter, blockId, sourceURL, offset, count, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.blockBlobContext.stageBlockFromURL(blockId, 0, sourceURL, {
                            abortSignal: aborter,
                            leaseAccessConditions: options.leaseAccessConditions,
                            sourceContentMD5: options.sourceContentMD5,
                            sourceContentCrc64: options.sourceContentCrc64,
                            sourceRange: offset === 0 && !count ? undefined : rangeToString({ offset: offset, count: count }),
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * Writes a blob by specifying the list of block IDs that make up the blob.
         * In order to be written as part of a blob, a block must have been successfully written
         * to the server in a prior stageBlock operation. You can call commitBlockList to update a blob
         * by uploading only those blocks that have changed, then committing the new and existing
         * blocks together. Any blocks not specified in the block list and permanently deleted.
         * @see https://docs.microsoft.com/rest/api/storageservices/put-block-list
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string[]} blocks  Array of 64-byte value that is base64-encoded
         * @param {IBlockBlobCommitBlockListOptions} [options]
         * @returns {Promise<Models.BlockBlobCommitBlockListResponse>}
         * @memberof BlockBlobURL
         */
        BlockBlobURL.prototype.commitBlockList = function (aborter, blocks, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.blockBlobContext.commitBlockList({ latest: blocks }, {
                            abortSignal: aborter,
                            blobHTTPHeaders: options.blobHTTPHeaders,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            metadata: options.metadata,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            cpkInfo: options.customerProvidedKey,
                            tier: toAccessTier(options.tier)
                        })];
                });
            });
        };
        /**
         * Returns the list of blocks that have been uploaded as part of a block blob
         * using the specified block list filter.
         * @see https://docs.microsoft.com/rest/api/storageservices/get-block-list
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {Models.BlockListType} listType
         * @param {IBlockBlobGetBlockListOptions} [options]
         * @returns {Promise<Models.BlockBlobGetBlockListResponse>}
         * @memberof BlockBlobURL
         */
        BlockBlobURL.prototype.getBlockList = function (aborter, listType, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                var res;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.blockBlobContext.getBlockList(listType, {
                                abortSignal: aborter,
                                leaseAccessConditions: options.leaseAccessConditions
                            })];
                        case 1:
                            res = _a.sent();
                            if (!res.committedBlocks) {
                                res.committedBlocks = [];
                            }
                            if (!res.uncommittedBlocks) {
                                res.uncommittedBlocks = [];
                            }
                            return [2 /*return*/, res];
                    }
                });
            });
        };
        return BlockBlobURL;
    }(BlobURL));

    /**
     * A ContainerURL represents a URL to the Azure Storage container allowing you to manipulate its blobs.
     *
     * @export
     * @class ContainerURL
     * @extends {StorageURL}
     */
    var ContainerURL = /** @class */ (function (_super) {
        __extends(ContainerURL, _super);
        /**
         * Creates an instance of ContainerURL.
         * @param {string} url A URL string pointing to Azure Storage blob container, such as
         *                     "https://myaccount.blob.core.windows.net/mycontainer". You can
         *                     append a SAS if using AnonymousCredential, such as
         *                     "https://myaccount.blob.core.windows.net/mycontainer?sasString".
         * @param {Pipeline} pipeline Call StorageURL.newPipeline() to create a default
         *                            pipeline, or provide a customized pipeline.
         * @memberof ContainerURL
         */
        function ContainerURL(url, pipeline) {
            var _this = _super.call(this, url, pipeline) || this;
            _this.containerContext = new Container(_this.storageClientContext);
            return _this;
        }
        /**
         * Creates a ContainerURL object from ServiceURL
         *
         * @param serviceURL A ServiceURL object
         * @param containerName A container name
         */
        ContainerURL.fromServiceURL = function (serviceURL, containerName) {
            return new ContainerURL(appendToURLPath(serviceURL.url, encodeURIComponent(containerName)), serviceURL.pipeline);
        };
        /**
         * Creates a new ContainerURL object identical to the source but with the
         * specified request policy pipeline.
         *
         * @param {Pipeline} pipeline
         * @returns {ContainerURL}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.withPipeline = function (pipeline) {
            return new ContainerURL(this.url, pipeline);
        };
        /**
         * Creates a new container under the specified account. If the container with
         * the same name already exists, the operation fails.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/create-container
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {IContainerCreateOptions} [options]
         * @returns {Promise<Models.ContainerCreateResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.create = function (aborter, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    // Spread operator in destructuring assignments,
                    // this will filter out unwanted properties from the response object into result object
                    return [2 /*return*/, this.containerContext.create(__assign({}, options, { abortSignal: aborter }))];
                });
            });
        };
        /**
         * Returns all user-defined metadata and system properties for the specified
         * container. The data returned does not include the container's list of blobs.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/get-container-properties
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {IContainersGetPropertiesOptions} [options]
         * @returns {Promise<Models.ContainerGetPropertiesResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.getProperties = function (aborter, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    if (!options.leaseAccessConditions) {
                        options.leaseAccessConditions = {};
                    }
                    return [2 /*return*/, this.containerContext.getProperties(__assign({ abortSignal: aborter }, options.leaseAccessConditions))];
                });
            });
        };
        /**
         * Marks the specified container for deletion. The container and any blobs
         * contained within it are later deleted during garbage collection.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/delete-container
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {Models.ContainersDeleteMethodOptionalParams} [options]
         * @returns {Promise<Models.ContainerDeleteResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.delete = function (aborter, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    if (!options.containerAccessConditions) {
                        options.containerAccessConditions = {};
                    }
                    if (!options.containerAccessConditions.modifiedAccessConditions) {
                        options.containerAccessConditions.modifiedAccessConditions = {};
                    }
                    if (!options.containerAccessConditions.leaseAccessConditions) {
                        options.containerAccessConditions.leaseAccessConditions = {};
                    }
                    if ((options.containerAccessConditions.modifiedAccessConditions.ifMatch &&
                        options.containerAccessConditions.modifiedAccessConditions.ifMatch !== ETagNone) ||
                        (options.containerAccessConditions.modifiedAccessConditions.ifNoneMatch &&
                            options.containerAccessConditions.modifiedAccessConditions.ifNoneMatch !== ETagNone)) {
                        throw new RangeError("the IfMatch and IfNoneMatch access conditions must have their default\
        values because they are ignored by the service");
                    }
                    return [2 /*return*/, this.containerContext.deleteMethod({
                            abortSignal: aborter,
                            leaseAccessConditions: options.containerAccessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.containerAccessConditions.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * Sets one or more user-defined name-value pairs for the specified container.
         *
         * If no option provided, or no metadata defined in the parameter, the container
         * metadata will be removed.
         *
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/set-container-metadata
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {IMetadata} [metadata] Replace existing metadata with this value.
         *                               If no value provided the existing metadata will be removed.
         * @param {IContainerSetMetadataOptions} [options]
         * @returns {Promise<Models.ContainerSetMetadataResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.setMetadata = function (aborter, metadata, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    if (!options.containerAccessConditions) {
                        options.containerAccessConditions = {};
                    }
                    if (!options.containerAccessConditions.modifiedAccessConditions) {
                        options.containerAccessConditions.modifiedAccessConditions = {};
                    }
                    if (!options.containerAccessConditions.leaseAccessConditions) {
                        options.containerAccessConditions.leaseAccessConditions = {};
                    }
                    if (options.containerAccessConditions.modifiedAccessConditions.ifUnmodifiedSince ||
                        (options.containerAccessConditions.modifiedAccessConditions.ifMatch &&
                            options.containerAccessConditions.modifiedAccessConditions.ifMatch !== ETagNone) ||
                        (options.containerAccessConditions.modifiedAccessConditions.ifNoneMatch &&
                            options.containerAccessConditions.modifiedAccessConditions.ifNoneMatch !== ETagNone)) {
                        throw new RangeError("the IfUnmodifiedSince, IfMatch, and IfNoneMatch must have their default values\
        because they are ignored by the blob service");
                    }
                    return [2 /*return*/, this.containerContext.setMetadata({
                            abortSignal: aborter,
                            leaseAccessConditions: options.containerAccessConditions.leaseAccessConditions,
                            metadata: metadata,
                            modifiedAccessConditions: options.containerAccessConditions.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * Gets the permissions for the specified container. The permissions indicate
         * whether container data may be accessed publicly.
         *
         * WARNING: JavaScript Date will potential lost precision when parsing start and expiry string.
         * For example, new Date("2018-12-31T03:44:23.8827891Z").toISOString() will get "2018-12-31T03:44:23.882Z".
         *
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/get-container-acl
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {IContainerGetAccessPolicyOptions} [options]
         * @returns {Promise<ContainerGetAccessPolicyResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.getAccessPolicy = function (aborter, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                var response, res, _i, response_1, identifier, accessPolicy;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!options.leaseAccessConditions) {
                                options.leaseAccessConditions = {};
                            }
                            return [4 /*yield*/, this.containerContext.getAccessPolicy({
                                    abortSignal: aborter,
                                    leaseAccessConditions: options.leaseAccessConditions
                                })];
                        case 1:
                            response = _a.sent();
                            res = {
                                _response: response._response,
                                blobPublicAccess: response.blobPublicAccess,
                                date: response.date,
                                eTag: response.eTag,
                                errorCode: response.errorCode,
                                lastModified: response.lastModified,
                                requestId: response.requestId,
                                clientRequestId: response.clientRequestId,
                                signedIdentifiers: [],
                                version: response.version
                            };
                            for (_i = 0, response_1 = response; _i < response_1.length; _i++) {
                                identifier = response_1[_i];
                                accessPolicy = {
                                    permission: identifier.accessPolicy.permission,
                                };
                                if (identifier.accessPolicy.expiry) {
                                    accessPolicy.expiry = new Date(identifier.accessPolicy.expiry);
                                }
                                if (identifier.accessPolicy.start) {
                                    accessPolicy.start = new Date(identifier.accessPolicy.start);
                                }
                                res.signedIdentifiers.push({
                                    accessPolicy: accessPolicy,
                                    id: identifier.id
                                });
                            }
                            return [2 /*return*/, res];
                    }
                });
            });
        };
        /**
         * Sets the permissions for the specified container. The permissions indicate
         * whether blobs in a container may be accessed publicly.
         *
         * When you set permissions for a container, the existing permissions are replaced.
         * If no access or containerAcl provided, the existing container ACL will be
         * removed.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/set-container-acl
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {PublicAccessType} [access]
         * @param {ISignedIdentifier[]} [containerAcl]
         * @param {IContainerSetAccessPolicyOptions} [options]
         * @returns {Promise<Models.ContainerSetAccessPolicyResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.setAccessPolicy = function (aborter, access, containerAcl, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                var acl, _i, _a, identifier;
                return __generator(this, function (_b) {
                    options.containerAccessConditions = options.containerAccessConditions || {};
                    acl = [];
                    for (_i = 0, _a = containerAcl || []; _i < _a.length; _i++) {
                        identifier = _a[_i];
                        acl.push({
                            accessPolicy: {
                                expiry: identifier.accessPolicy.expiry ? truncatedISO8061Date(identifier.accessPolicy.expiry) : "",
                                permission: identifier.accessPolicy.permission,
                                start: identifier.accessPolicy.start ? truncatedISO8061Date(identifier.accessPolicy.start) : ""
                            },
                            id: identifier.id
                        });
                    }
                    return [2 /*return*/, this.containerContext.setAccessPolicy({
                            abortSignal: aborter,
                            access: access,
                            containerAcl: acl,
                            leaseAccessConditions: options.containerAccessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.containerAccessConditions.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * Establishes and manages a lock on a container for delete operations.
         * The lock duration can be 15 to 60 seconds, or can be infinite.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/lease-container
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} proposedLeaseId Can be specified in any valid GUID string format
         * @param {number} duration Must be between 15 to 60 seconds, or infinite (-1)
         * @param {IContainerAcquireLeaseOptions} [options]
         * @returns {Promise<Models.ContainerAcquireLeaseResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.acquireLease = function (aborter, proposedLeaseId, duration, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.containerContext.acquireLease({
                            abortSignal: aborter,
                            duration: duration,
                            modifiedAccessConditions: options.modifiedAccessConditions,
                            proposedLeaseId: proposedLeaseId
                        })];
                });
            });
        };
        /**
         * To free the lease if it is no longer needed so that another client may
         * immediately acquire a lease against the container.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/lease-container
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} leaseId
         * @param {IContainerReleaseLeaseOptions} [options]
         * @returns {Promise<Models.ContainerReleaseLeaseResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.releaseLease = function (aborter, leaseId, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.containerContext.releaseLease(leaseId, {
                            abortSignal: aborter,
                            modifiedAccessConditions: options.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * To renew an existing lease.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/lease-container
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} leaseId
         * @param {IContainerRenewLeaseOptions} [options]
         * @returns {Promise<Models.ContainerRenewLeaseResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.renewLease = function (aborter, leaseId, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.containerContext.renewLease(leaseId, {
                            abortSignal: aborter,
                            modifiedAccessConditions: options.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * To end the lease but ensure that another client cannot acquire a new lease
         * until the current lease period has expired.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/lease-container
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {number} period break period
         * @param {IContainerBreakLeaseOptions} [options]
         * @returns {Promise<Models.ContainerBreakLeaseResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.breakLease = function (aborter, period, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.containerContext.breakLease({
                            abortSignal: aborter,
                            breakPeriod: period,
                            modifiedAccessConditions: options.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * To change the ID of an existing lease.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/lease-container
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} leaseId
         * @param {string} proposedLeaseId
         * @param {IContainerChangeLeaseOptions} [options]
         * @returns {Promise<Models.ContainerChangeLeaseResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.changeLease = function (aborter, leaseId, proposedLeaseId, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.containerContext.changeLease(leaseId, proposedLeaseId, {
                            abortSignal: aborter,
                            modifiedAccessConditions: options.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * listBlobFlatSegment returns a single segment of blobs starting from the
         * specified Marker. Use an empty Marker to start enumeration from the beginning.
         * After getting a segment, process it, and then call ListBlobsFlatSegment again
         * (passing the the previously-returned Marker) to get the next segment.
         * @see https://docs.microsoft.com/rest/api/storageservices/list-blobs
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} [marker]
         * @param {IContainerListBlobsSegmentOptions} [options]
         * @returns {Promise<Models.ContainerListBlobFlatSegmentResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.listBlobFlatSegment = function (aborter, marker, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.containerContext.listBlobFlatSegment(__assign({ abortSignal: aborter, marker: marker }, options))];
                });
            });
        };
        /**
         * listBlobHierarchySegment returns a single segment of blobs starting from
         * the specified Marker. Use an empty Marker to start enumeration from the
         * beginning. After getting a segment, process it, and then call ListBlobsHierarchicalSegment
         * again (passing the the previously-returned Marker) to get the next segment.
         * @see https://docs.microsoft.com/rest/api/storageservices/list-blobs
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} delimiter
         * @param {IContainerListBlobsSegmentOptions} [options]
         * @returns {Promise<Models.ContainerListBlobHierarchySegmentResponse>}
         * @memberof ContainerURL
         */
        ContainerURL.prototype.listBlobHierarchySegment = function (aborter, delimiter, marker, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.containerContext.listBlobHierarchySegment(delimiter, __assign({ abortSignal: aborter, marker: marker }, options))];
                });
            });
        };
        return ContainerURL;
    }(StorageURL));

    /**
     * TokenCredentialPolicy is a policy used to sign HTTP request with a token.
     * Such as an OAuth bearer token.
     *
     * @export
     * @class TokenCredentialPolicy
     * @extends {CredentialPolicy}
     */
    var TokenCredentialPolicy = /** @class */ (function (_super) {
        __extends(TokenCredentialPolicy, _super);
        /**
         * Creates an instance of TokenCredentialPolicy.
         * @param {RequestPolicy} nextPolicy
         * @param {RequestPolicyOptions} options
         * @param {TokenCredential} tokenCredential
         * @memberof TokenCredentialPolicy
         */
        function TokenCredentialPolicy(nextPolicy, options, tokenCredential) {
            var _this = _super.call(this, nextPolicy, options) || this;
            _this.tokenCredential = tokenCredential;
            _this.authorizationScheme = HeaderConstants.AUTHORIZATION_SCHEME;
            return _this;
        }
        /**
         * Sign request with token.
         *
         * @protected
         * @param {WebResource} request
         * @returns {WebResource}
         * @memberof TokenCredentialPolicy
         */
        TokenCredentialPolicy.prototype.signRequest = function (request) {
            if (!request.headers) {
                request.headers = new HttpHeaders();
            }
            request.headers.set(HeaderConstants.AUTHORIZATION, this.authorizationScheme + " " + this.tokenCredential.token);
            return request;
        };
        return TokenCredentialPolicy;
    }(CredentialPolicy));

    /**
     * TokenCredential is a Credential used to generate a TokenCredentialPolicy.
     * Renew token by setting a new token string value to token property.
     *
     * @example
     *  const tokenCredential = new TokenCredential("token");
     *  const pipeline = StorageURL.newPipeline(tokenCredential);
     *
     *  // List containers
     *  const serviceURL = new ServiceURL("https://mystorageaccount.blob.core.windows.net", pipeline);
     *
     *  // Set up a timer to refresh the token
     *  const timerID = setInterval(() => {
     *    // Update token by accessing to public tokenCredential.token
     *    tokenCredential.token = "updatedToken";
     *    // WARNING: Timer must be manually stopped! It will forbid GC of tokenCredential
     *    if (shouldStop()) {
     *      clearInterval(timerID);
     *    }
     *  }, 60 * 60 * 1000); // Set an interval time before your token expired
     * @export
     * @class TokenCredential
     * @extends {Credential}
     *
     */
    var TokenCredential = /** @class */ (function (_super) {
        __extends(TokenCredential, _super);
        /**
         * Creates an instance of TokenCredential.
         * @param {string} token
         * @memberof TokenCredential
         */
        function TokenCredential(token) {
            var _this = _super.call(this) || this;
            _this.token = token;
            return _this;
        }
        /**
         * Creates a TokenCredentialPolicy object.
         *
         * @param {RequestPolicy} nextPolicy
         * @param {RequestPolicyOptions} options
         * @returns {TokenCredentialPolicy}
         * @memberof TokenCredential
         */
        TokenCredential.prototype.create = function (nextPolicy, options) {
            return new TokenCredentialPolicy(nextPolicy, options, this);
        };
        return TokenCredential;
    }(Credential));

    // Copyright Joyent, Inc. and other Node contributors.

    var R = typeof Reflect === 'object' ? Reflect : null;
    var ReflectApply = R && typeof R.apply === 'function'
      ? R.apply
      : function ReflectApply(target, receiver, args) {
        return Function.prototype.apply.call(target, receiver, args);
      };

    var ReflectOwnKeys;
    if (R && typeof R.ownKeys === 'function') {
      ReflectOwnKeys = R.ownKeys;
    } else if (Object.getOwnPropertySymbols) {
      ReflectOwnKeys = function ReflectOwnKeys(target) {
        return Object.getOwnPropertyNames(target)
          .concat(Object.getOwnPropertySymbols(target));
      };
    } else {
      ReflectOwnKeys = function ReflectOwnKeys(target) {
        return Object.getOwnPropertyNames(target);
      };
    }

    function ProcessEmitWarning(warning) {
      if (console && console.warn) console.warn(warning);
    }

    var NumberIsNaN = Number.isNaN || function NumberIsNaN(value) {
      return value !== value;
    };

    function EventEmitter() {
      EventEmitter.init.call(this);
    }
    var events = EventEmitter;

    // Backwards-compat with node 0.10.x
    EventEmitter.EventEmitter = EventEmitter;

    EventEmitter.prototype._events = undefined;
    EventEmitter.prototype._eventsCount = 0;
    EventEmitter.prototype._maxListeners = undefined;

    // By default EventEmitters will print a warning if more than 10 listeners are
    // added to it. This is a useful default which helps finding memory leaks.
    var defaultMaxListeners = 10;

    Object.defineProperty(EventEmitter, 'defaultMaxListeners', {
      enumerable: true,
      get: function() {
        return defaultMaxListeners;
      },
      set: function(arg) {
        if (typeof arg !== 'number' || arg < 0 || NumberIsNaN(arg)) {
          throw new RangeError('The value of "defaultMaxListeners" is out of range. It must be a non-negative number. Received ' + arg + '.');
        }
        defaultMaxListeners = arg;
      }
    });

    EventEmitter.init = function() {

      if (this._events === undefined ||
          this._events === Object.getPrototypeOf(this)._events) {
        this._events = Object.create(null);
        this._eventsCount = 0;
      }

      this._maxListeners = this._maxListeners || undefined;
    };

    // Obviously not all Emitters should be limited to 10. This function allows
    // that to be increased. Set to zero for unlimited.
    EventEmitter.prototype.setMaxListeners = function setMaxListeners(n) {
      if (typeof n !== 'number' || n < 0 || NumberIsNaN(n)) {
        throw new RangeError('The value of "n" is out of range. It must be a non-negative number. Received ' + n + '.');
      }
      this._maxListeners = n;
      return this;
    };

    function $getMaxListeners(that) {
      if (that._maxListeners === undefined)
        return EventEmitter.defaultMaxListeners;
      return that._maxListeners;
    }

    EventEmitter.prototype.getMaxListeners = function getMaxListeners() {
      return $getMaxListeners(this);
    };

    EventEmitter.prototype.emit = function emit(type) {
      var args = [];
      for (var i = 1; i < arguments.length; i++) args.push(arguments[i]);
      var doError = (type === 'error');

      var events = this._events;
      if (events !== undefined)
        doError = (doError && events.error === undefined);
      else if (!doError)
        return false;

      // If there is no 'error' event listener then throw.
      if (doError) {
        var er;
        if (args.length > 0)
          er = args[0];
        if (er instanceof Error) {
          // Note: The comments on the `throw` lines are intentional, they show
          // up in Node's output if this results in an unhandled exception.
          throw er; // Unhandled 'error' event
        }
        // At least give some kind of context to the user
        var err = new Error('Unhandled error.' + (er ? ' (' + er.message + ')' : ''));
        err.context = er;
        throw err; // Unhandled 'error' event
      }

      var handler = events[type];

      if (handler === undefined)
        return false;

      if (typeof handler === 'function') {
        ReflectApply(handler, this, args);
      } else {
        var len = handler.length;
        var listeners = arrayClone(handler, len);
        for (var i = 0; i < len; ++i)
          ReflectApply(listeners[i], this, args);
      }

      return true;
    };

    function _addListener(target, type, listener, prepend) {
      var m;
      var events;
      var existing;

      if (typeof listener !== 'function') {
        throw new TypeError('The "listener" argument must be of type Function. Received type ' + typeof listener);
      }

      events = target._events;
      if (events === undefined) {
        events = target._events = Object.create(null);
        target._eventsCount = 0;
      } else {
        // To avoid recursion in the case that type === "newListener"! Before
        // adding it to the listeners, first emit "newListener".
        if (events.newListener !== undefined) {
          target.emit('newListener', type,
                      listener.listener ? listener.listener : listener);

          // Re-assign `events` because a newListener handler could have caused the
          // this._events to be assigned to a new object
          events = target._events;
        }
        existing = events[type];
      }

      if (existing === undefined) {
        // Optimize the case of one listener. Don't need the extra array object.
        existing = events[type] = listener;
        ++target._eventsCount;
      } else {
        if (typeof existing === 'function') {
          // Adding the second element, need to change to array.
          existing = events[type] =
            prepend ? [listener, existing] : [existing, listener];
          // If we've already got an array, just append.
        } else if (prepend) {
          existing.unshift(listener);
        } else {
          existing.push(listener);
        }

        // Check for listener leak
        m = $getMaxListeners(target);
        if (m > 0 && existing.length > m && !existing.warned) {
          existing.warned = true;
          // No error code for this since it is a Warning
          // eslint-disable-next-line no-restricted-syntax
          var w = new Error('Possible EventEmitter memory leak detected. ' +
                              existing.length + ' ' + String(type) + ' listeners ' +
                              'added. Use emitter.setMaxListeners() to ' +
                              'increase limit');
          w.name = 'MaxListenersExceededWarning';
          w.emitter = target;
          w.type = type;
          w.count = existing.length;
          ProcessEmitWarning(w);
        }
      }

      return target;
    }

    EventEmitter.prototype.addListener = function addListener(type, listener) {
      return _addListener(this, type, listener, false);
    };

    EventEmitter.prototype.on = EventEmitter.prototype.addListener;

    EventEmitter.prototype.prependListener =
        function prependListener(type, listener) {
          return _addListener(this, type, listener, true);
        };

    function onceWrapper() {
      var args = [];
      for (var i = 0; i < arguments.length; i++) args.push(arguments[i]);
      if (!this.fired) {
        this.target.removeListener(this.type, this.wrapFn);
        this.fired = true;
        ReflectApply(this.listener, this.target, args);
      }
    }

    function _onceWrap(target, type, listener) {
      var state = { fired: false, wrapFn: undefined, target: target, type: type, listener: listener };
      var wrapped = onceWrapper.bind(state);
      wrapped.listener = listener;
      state.wrapFn = wrapped;
      return wrapped;
    }

    EventEmitter.prototype.once = function once(type, listener) {
      if (typeof listener !== 'function') {
        throw new TypeError('The "listener" argument must be of type Function. Received type ' + typeof listener);
      }
      this.on(type, _onceWrap(this, type, listener));
      return this;
    };

    EventEmitter.prototype.prependOnceListener =
        function prependOnceListener(type, listener) {
          if (typeof listener !== 'function') {
            throw new TypeError('The "listener" argument must be of type Function. Received type ' + typeof listener);
          }
          this.prependListener(type, _onceWrap(this, type, listener));
          return this;
        };

    // Emits a 'removeListener' event if and only if the listener was removed.
    EventEmitter.prototype.removeListener =
        function removeListener(type, listener) {
          var list, events, position, i, originalListener;

          if (typeof listener !== 'function') {
            throw new TypeError('The "listener" argument must be of type Function. Received type ' + typeof listener);
          }

          events = this._events;
          if (events === undefined)
            return this;

          list = events[type];
          if (list === undefined)
            return this;

          if (list === listener || list.listener === listener) {
            if (--this._eventsCount === 0)
              this._events = Object.create(null);
            else {
              delete events[type];
              if (events.removeListener)
                this.emit('removeListener', type, list.listener || listener);
            }
          } else if (typeof list !== 'function') {
            position = -1;

            for (i = list.length - 1; i >= 0; i--) {
              if (list[i] === listener || list[i].listener === listener) {
                originalListener = list[i].listener;
                position = i;
                break;
              }
            }

            if (position < 0)
              return this;

            if (position === 0)
              list.shift();
            else {
              spliceOne(list, position);
            }

            if (list.length === 1)
              events[type] = list[0];

            if (events.removeListener !== undefined)
              this.emit('removeListener', type, originalListener || listener);
          }

          return this;
        };

    EventEmitter.prototype.off = EventEmitter.prototype.removeListener;

    EventEmitter.prototype.removeAllListeners =
        function removeAllListeners(type) {
          var listeners, events, i;

          events = this._events;
          if (events === undefined)
            return this;

          // not listening for removeListener, no need to emit
          if (events.removeListener === undefined) {
            if (arguments.length === 0) {
              this._events = Object.create(null);
              this._eventsCount = 0;
            } else if (events[type] !== undefined) {
              if (--this._eventsCount === 0)
                this._events = Object.create(null);
              else
                delete events[type];
            }
            return this;
          }

          // emit removeListener for all listeners on all events
          if (arguments.length === 0) {
            var keys = Object.keys(events);
            var key;
            for (i = 0; i < keys.length; ++i) {
              key = keys[i];
              if (key === 'removeListener') continue;
              this.removeAllListeners(key);
            }
            this.removeAllListeners('removeListener');
            this._events = Object.create(null);
            this._eventsCount = 0;
            return this;
          }

          listeners = events[type];

          if (typeof listeners === 'function') {
            this.removeListener(type, listeners);
          } else if (listeners !== undefined) {
            // LIFO order
            for (i = listeners.length - 1; i >= 0; i--) {
              this.removeListener(type, listeners[i]);
            }
          }

          return this;
        };

    function _listeners(target, type, unwrap) {
      var events = target._events;

      if (events === undefined)
        return [];

      var evlistener = events[type];
      if (evlistener === undefined)
        return [];

      if (typeof evlistener === 'function')
        return unwrap ? [evlistener.listener || evlistener] : [evlistener];

      return unwrap ?
        unwrapListeners(evlistener) : arrayClone(evlistener, evlistener.length);
    }

    EventEmitter.prototype.listeners = function listeners(type) {
      return _listeners(this, type, true);
    };

    EventEmitter.prototype.rawListeners = function rawListeners(type) {
      return _listeners(this, type, false);
    };

    EventEmitter.listenerCount = function(emitter, type) {
      if (typeof emitter.listenerCount === 'function') {
        return emitter.listenerCount(type);
      } else {
        return listenerCount.call(emitter, type);
      }
    };

    EventEmitter.prototype.listenerCount = listenerCount;
    function listenerCount(type) {
      var events = this._events;

      if (events !== undefined) {
        var evlistener = events[type];

        if (typeof evlistener === 'function') {
          return 1;
        } else if (evlistener !== undefined) {
          return evlistener.length;
        }
      }

      return 0;
    }

    EventEmitter.prototype.eventNames = function eventNames() {
      return this._eventsCount > 0 ? ReflectOwnKeys(this._events) : [];
    };

    function arrayClone(arr, n) {
      var copy = new Array(n);
      for (var i = 0; i < n; ++i)
        copy[i] = arr[i];
      return copy;
    }

    function spliceOne(list, index) {
      for (; index + 1 < list.length; index++)
        list[index] = list[index + 1];
      list.pop();
    }

    function unwrapListeners(arr) {
      var ret = new Array(arr.length);
      for (var i = 0; i < ret.length; ++i) {
        ret[i] = arr[i].listener || arr[i];
      }
      return ret;
    }
    var events_1 = events.EventEmitter;

    /**
     * States for Batch.
     *
     * @enum {number}
     */
    var BatchStates;
    (function (BatchStates) {
        BatchStates[BatchStates["Good"] = 0] = "Good";
        BatchStates[BatchStates["Error"] = 1] = "Error";
    })(BatchStates || (BatchStates = {}));
    /**
     * Batch provides basic parallel execution with concurrency limits.
     * Will stop execute left operations when one of the executed operation throws an error.
     * But Batch cannot cancel ongoing operations, you need to cancel them by yourself.
     *
     * @export
     * @class Batch
     */
    var Batch = /** @class */ (function () {
        /**
         * Creates an instance of Batch.
         * @param {number} [concurrency=5]
         * @memberof Batch
         */
        function Batch(concurrency) {
            if (concurrency === void 0) { concurrency = 5; }
            /**
             * Number of active operations under execution.
             *
             * @private
             * @type {number}
             * @memberof Batch
             */
            this.actives = 0;
            /**
             * Number of completed operations under execution.
             *
             * @private
             * @type {number}
             * @memberof Batch
             */
            this.completed = 0;
            /**
             * Offset of next operation to be executed.
             *
             * @private
             * @type {number}
             * @memberof Batch
             */
            this.offset = 0;
            /**
             * Operation array to be executed.
             *
             * @private
             * @type {Operation[]}
             * @memberof Batch
             */
            this.operations = [];
            /**
             * States of Batch. When an error happens, state will turn into error.
             * Batch will stop execute left operations.
             *
             * @private
             * @type {BatchStates}
             * @memberof Batch
             */
            this.state = BatchStates.Good;
            if (concurrency < 1) {
                throw new RangeError("concurrency must be larger than 0");
            }
            this.concurrency = concurrency;
            this.emitter = new events_1();
        }
        /**
         * Add a operation into queue.
         *
         * @param {Operation} operation
         * @memberof Batch
         */
        Batch.prototype.addOperation = function (operation) {
            var _this = this;
            this.operations.push(function () { return __awaiter(_this, void 0, void 0, function () {
                var error_1;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            _a.trys.push([0, 2, , 3]);
                            this.actives++;
                            return [4 /*yield*/, operation()];
                        case 1:
                            _a.sent();
                            this.actives--;
                            this.completed++;
                            this.parallelExecute();
                            return [3 /*break*/, 3];
                        case 2:
                            error_1 = _a.sent();
                            this.emitter.emit("error", error_1);
                            return [3 /*break*/, 3];
                        case 3: return [2 /*return*/];
                    }
                });
            }); });
        };
        /**
         * Start execute operations in the queue.
         *
         * @returns {Promise<void>}
         * @memberof Batch
         */
        Batch.prototype.do = function () {
            return __awaiter(this, void 0, void 0, function () {
                var _this = this;
                return __generator(this, function (_a) {
                    this.parallelExecute();
                    return [2 /*return*/, new Promise(function (resolve, reject) {
                            _this.emitter.on("finish", resolve);
                            _this.emitter.on("error", function (error) {
                                _this.state = BatchStates.Error;
                                reject(error);
                            });
                        })];
                });
            });
        };
        /**
         * Get next operation to be executed. Return null when reaching ends.
         *
         * @private
         * @returns {(Operation | null)}
         * @memberof Batch
         */
        Batch.prototype.nextOperation = function () {
            if (this.offset < this.operations.length) {
                return this.operations[this.offset++];
            }
            return null;
        };
        /**
         * Start execute operations. One one the most important difference between
         * this method with do() is that do() wraps as an sync method.
         *
         * @private
         * @returns {void}
         * @memberof Batch
         */
        Batch.prototype.parallelExecute = function () {
            if (this.state === BatchStates.Error) {
                return;
            }
            if (this.completed >= this.operations.length) {
                this.emitter.emit("finish");
                return;
            }
            while (this.actives < this.concurrency) {
                var operation = this.nextOperation();
                if (operation) {
                    operation();
                }
                else {
                    return;
                }
            }
        };
        return Batch;
    }());

    /**
     * ONLY AVAILABLE IN BROWSERS.
     *
     * Uploads a browser Blob/File/ArrayBuffer/ArrayBufferView object to block blob.
     *
     * When buffer length <= 256MB, this method will use 1 upload call to finish the upload.
     * Otherwise, this method will call stageBlock to upload blocks, and finally call commitBlockList
     * to commit the block list.
     *
     * @export
     * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
     *                          goto documents of Aborter for more examples about request cancellation
     * @param {Blob | ArrayBuffer | ArrayBufferView} browserData Blob, File, ArrayBuffer or ArrayBufferView
     * @param {BlockBlobURL} blockBlobURL
     * @param {IUploadToBlockBlobOptions} [options]
     * @returns {Promise<BlobUploadCommonResponse>}
     */
    function uploadBrowserDataToBlockBlob(aborter, browserData, blockBlobURL, options) {
        return __awaiter(this, void 0, void 0, function () {
            var browserBlob;
            return __generator(this, function (_a) {
                browserBlob = new Blob([browserData]);
                return [2 /*return*/, UploadSeekableBlobToBlockBlob(aborter, function (offset, size) {
                        return browserBlob.slice(offset, offset + size);
                    }, browserBlob.size, blockBlobURL, options)];
            });
        });
    }
    /**
     * ONLY AVAILABLE IN BROWSERS.
     *
     * Uploads a browser Blob object to block blob. Requires a blobFactory as the data source,
     * which need to return a Blob object with the offset and size provided.
     *
     * When buffer length <= 256MB, this method will use 1 upload call to finish the upload.
     * Otherwise, this method will call stageBlock to upload blocks, and finally call commitBlockList
     * to commit the block list.
     *
     * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
     *                          goto documents of Aborter for more examples about request cancellation
     * @param {(offset: number, size: number) => Blob} blobFactory
     * @param {number} size
     * @param {BlockBlobURL} blockBlobURL
     * @param {IUploadToBlockBlobOptions} [options]
     * @returns {Promise<BlobUploadCommonResponse>}
     */
    function UploadSeekableBlobToBlockBlob(aborter, blobFactory, size, blockBlobURL, options) {
        if (options === void 0) { options = {}; }
        return __awaiter(this, void 0, void 0, function () {
            var numBlocks, blockList, blockIDPrefix, transferProgress, batch, _loop_1, i;
            var _this = this;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        if (!options.blockSize) {
                            options.blockSize = 0;
                        }
                        if (options.blockSize < 0 || options.blockSize > BLOCK_BLOB_MAX_STAGE_BLOCK_BYTES) {
                            throw new RangeError("blockSize option must be >= 0 and <= " + BLOCK_BLOB_MAX_STAGE_BLOCK_BYTES);
                        }
                        if (options.maxSingleShotSize !== 0 && !options.maxSingleShotSize) {
                            options.maxSingleShotSize = BLOCK_BLOB_MAX_UPLOAD_BLOB_BYTES;
                        }
                        if (options.maxSingleShotSize < 0 ||
                            options.maxSingleShotSize > BLOCK_BLOB_MAX_UPLOAD_BLOB_BYTES) {
                            throw new RangeError("maxSingleShotSize option must be >= 0 and <= " + BLOCK_BLOB_MAX_UPLOAD_BLOB_BYTES);
                        }
                        if (options.blockSize === 0) {
                            if (size > BLOCK_BLOB_MAX_STAGE_BLOCK_BYTES * BLOCK_BLOB_MAX_BLOCKS) {
                                throw new RangeError(size + " is too larger to upload to a block blob.");
                            }
                            if (size > options.maxSingleShotSize) {
                                options.blockSize = Math.ceil(size / BLOCK_BLOB_MAX_BLOCKS);
                                if (options.blockSize < DEFAULT_BLOB_DOWNLOAD_BLOCK_BYTES) {
                                    options.blockSize = DEFAULT_BLOB_DOWNLOAD_BLOCK_BYTES;
                                }
                            }
                        }
                        if (!options.blobHTTPHeaders) {
                            options.blobHTTPHeaders = {};
                        }
                        if (!options.blobAccessConditions) {
                            options.blobAccessConditions = {};
                        }
                        if (size <= options.maxSingleShotSize) {
                            return [2 /*return*/, blockBlobURL.upload(aborter, blobFactory(0, size), size, options)];
                        }
                        numBlocks = Math.floor((size - 1) / options.blockSize) + 1;
                        if (numBlocks > BLOCK_BLOB_MAX_BLOCKS) {
                            throw new RangeError("The buffer's size is too big or the BlockSize is too small;" +
                                ("the number of blocks must be <= " + BLOCK_BLOB_MAX_BLOCKS));
                        }
                        blockList = [];
                        blockIDPrefix = generateUuid();
                        transferProgress = 0;
                        batch = new Batch(options.parallelism);
                        _loop_1 = function (i) {
                            batch.addOperation(function () { return __awaiter(_this, void 0, void 0, function () {
                                var blockID, start, end, contentLength;
                                return __generator(this, function (_a) {
                                    switch (_a.label) {
                                        case 0:
                                            blockID = generateBlockID(blockIDPrefix, i);
                                            start = options.blockSize * i;
                                            end = i === numBlocks - 1 ? size : start + options.blockSize;
                                            contentLength = end - start;
                                            blockList.push(blockID);
                                            return [4 /*yield*/, blockBlobURL.stageBlock(aborter, blockID, blobFactory(start, contentLength), contentLength, {
                                                    leaseAccessConditions: options.blobAccessConditions.leaseAccessConditions
                                                })];
                                        case 1:
                                            _a.sent();
                                            // Update progress after block is successfully uploaded to server, in case of block trying
                                            // TODO: Hook with convenience layer progress event in finer level
                                            transferProgress += contentLength;
                                            if (options.progress) {
                                                options.progress({
                                                    loadedBytes: transferProgress
                                                });
                                            }
                                            return [2 /*return*/];
                                    }
                                });
                            }); });
                        };
                        for (i = 0; i < numBlocks; i++) {
                            _loop_1(i);
                        }
                        return [4 /*yield*/, batch.do()];
                    case 1:
                        _a.sent();
                        return [2 /*return*/, blockBlobURL.commitBlockList(aborter, blockList, options)];
                }
            });
        });
    }

    /**
     * PageBlobURL defines a set of operations applicable to page blobs.
     *
     * @export
     * @class PageBlobURL
     * @extends {StorageURL}
     */
    var PageBlobURL = /** @class */ (function (_super) {
        __extends(PageBlobURL, _super);
        /**
         * Creates an instance of PageBlobURL.
         * This method accepts an encoded URL or non-encoded URL pointing to a page blob.
         * Encoded URL string will NOT be escaped twice, only special characters in URL path will be escaped.
         * If a blob name includes ? or %, blob name must be encoded in the URL.
         *
         * @param {string} url A URL string pointing to Azure Storage page blob, such as
         *                     "https://myaccount.blob.core.windows.net/mycontainer/pageblob". You can
         *                     append a SAS if using AnonymousCredential, such as
         *                     "https://myaccount.blob.core.windows.net/mycontainer/pageblob?sasString".
         *                     This method accepts an encoded URL or non-encoded URL pointing to a blob.
         *                     Encoded URL string will NOT be escaped twice, only special characters in URL path will be escaped.
         *                     However, if a blob name includes ? or %, blob name must be encoded in the URL.
         *                     Such as a blob named "my?blob%", the URL should be "https://myaccount.blob.core.windows.net/mycontainer/my%3Fblob%25".
         * @param {Pipeline} pipeline Call StorageURL.newPipeline() to create a default
         *                            pipeline, or provide a customized pipeline.
         * @memberof PageBlobURL
         */
        function PageBlobURL(url, pipeline) {
            var _this = _super.call(this, url, pipeline) || this;
            _this.pageBlobContext = new PageBlob(_this.storageClientContext);
            return _this;
        }
        /**
         * Creates a PageBlobURL object from ContainerURL instance.
         *
         * @static
         * @param {ContainerURL} containerURL A ContainerURL object
         * @param {string} blobName A page blob name
         * @returns {PageBlobURL}
         * @memberof PageBlobURL
         */
        PageBlobURL.fromContainerURL = function (containerURL, blobName) {
            return new PageBlobURL(appendToURLPath(containerURL.url, encodeURIComponent(blobName)), containerURL.pipeline);
        };
        /**
         * Creates a PageBlobURL object from BlobURL instance.
         *
         * @static
         * @param {BlobURL} blobURL
         * @returns {PageBlobURL}
         * @memberof PageBlobURL
         */
        PageBlobURL.fromBlobURL = function (blobURL) {
            return new PageBlobURL(blobURL.url, blobURL.pipeline);
        };
        /**
         * Creates a new PageBlobURL object identical to the source but with the
         * specified request policy pipeline.
         *
         * @param {Pipeline} pipeline
         * @returns {PageBlobURL}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.withPipeline = function (pipeline) {
            return new PageBlobURL(this.url, pipeline);
        };
        /**
         * Creates a new PageBlobURL object identical to the source but with the
         * specified snapshot timestamp.
         * Provide "" will remove the snapshot and return a URL to the base blob.
         *
         * @param {string} snapshot
         * @returns {PageBlobURL}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.withSnapshot = function (snapshot) {
            return new PageBlobURL(setURLParameter(this.url, URLConstants.Parameters.SNAPSHOT, snapshot.length === 0 ? undefined : snapshot), this.pipeline);
        };
        /**
         * Creates a page blob of the specified length. Call uploadPages to upload data
         * data to a page blob.
         * @see https://docs.microsoft.com/rest/api/storageservices/put-blob
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {number} size
         * @param {IPageBlobCreateOptions} [options]
         * @returns {Promise<Models.PageBlobCreateResponse>}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.create = function (aborter, size, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.pageBlobContext.create(0, size, {
                            abortSignal: aborter,
                            blobHTTPHeaders: options.blobHTTPHeaders,
                            blobSequenceNumber: options.blobSequenceNumber,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            metadata: options.metadata,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            cpkInfo: options.customerProvidedKey,
                            tier: toAccessTier(options.tier)
                        })];
                });
            });
        };
        /**
         * Writes 1 or more pages to the page blob. The start and end offsets must be a multiple of 512.
         * @see https://docs.microsoft.com/rest/api/storageservices/put-page
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {HttpRequestBody} body
         * @param {number} offset Offset of destination page blob
         * @param {number} count Content length of the body, also number of bytes to be uploaded
         * @param {IPageBlobUploadPagesOptions} [options]
         * @returns {Promise<Models.PageBlobsUploadPagesResponse>}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.uploadPages = function (aborter, body, offset, count, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.pageBlobContext.uploadPages(body, count, {
                            abortSignal: aborter,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            onUploadProgress: options.progress,
                            range: rangeToString({ offset: offset, count: count }),
                            sequenceNumberAccessConditions: options.accessConditions.sequenceNumberAccessConditions,
                            transactionalContentMD5: options.transactionalContentMD5,
                            transactionalContentCrc64: options.transactionalContentCrc64,
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * The Upload Pages operation writes a range of pages to a page blob where the
         * contents are read from a URL.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/put-page-from-url
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} sourceURL Specify a URL to the copy source, Shared Access Signature(SAS) maybe needed for authentication
         * @param {number} sourceOffset The source offset to copy from. Pass 0 to copy from the beginning of source page blob
         * @param {number} destOffset Offset of destination page blob
         * @param {number} count Number of bytes to be uploaded from source page blob
         * @param {IPageBlobUploadPagesFromURLOptions} [options={}]
         * @returns {Promise<Models.PageBlobUploadPagesFromURLResponse>}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.uploadPagesFromURL = function (aborter, sourceURL, sourceOffset, destOffset, count, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    options.sourceModifiedAccessConditions = options.sourceModifiedAccessConditions || {};
                    ensureCpkIfSpecified(options.customerProvidedKey, this.isHttps);
                    return [2 /*return*/, this.pageBlobContext.uploadPagesFromURL(sourceURL, rangeToString({ offset: sourceOffset, count: count }), 0, rangeToString({ offset: destOffset, count: count }), {
                            abortSignal: aborter,
                            sourceContentMD5: options.sourceContentMD5,
                            sourceContentCrc64: options.sourceContentCrc64,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            sequenceNumberAccessConditions: options.accessConditions.sequenceNumberAccessConditions,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            sourceModifiedAccessConditions: {
                                sourceIfMatch: options.sourceModifiedAccessConditions.ifMatch,
                                sourceIfModifiedSince: options.sourceModifiedAccessConditions.ifModifiedSince,
                                sourceIfNoneMatch: options.sourceModifiedAccessConditions.ifNoneMatch,
                                sourceIfUnmodifiedSince: options.sourceModifiedAccessConditions.ifUnmodifiedSince
                            },
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * Frees the specified pages from the page blob.
         * @see https://docs.microsoft.com/rest/api/storageservices/put-page
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {number} offset
         * @param {number} count
         * @param {IPageBlobClearPagesOptions} [options]
         * @returns {Promise<Models.PageBlobClearPagesResponse>}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.clearPages = function (aborter, offset, count, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    return [2 /*return*/, this.pageBlobContext.clearPages(0, {
                            abortSignal: aborter,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            range: rangeToString({ offset: offset, count: count }),
                            sequenceNumberAccessConditions: options.accessConditions.sequenceNumberAccessConditions,
                            cpkInfo: options.customerProvidedKey
                        })];
                });
            });
        };
        /**
         * Returns the list of valid page ranges for a page blob or snapshot of a page blob.
         * @see https://docs.microsoft.com/rest/api/storageservices/get-page-ranges
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {number} offset
         * @param {number} count
         * @param {IPageBlobGetPageRangesOptions} [options]
         * @returns {Promise<Models.PageBlobGetPageRangesResponse>}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.getPageRanges = function (aborter, offset, count, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    return [2 /*return*/, this.pageBlobContext.getPageRanges({
                            abortSignal: aborter,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            range: rangeToString({ offset: offset, count: count })
                        })];
                });
            });
        };
        /**
         * Gets the collection of page ranges that differ between a specified snapshot and this page blob.
         * @see https://docs.microsoft.com/rest/api/storageservices/get-page-ranges
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {number} offset
         * @param {number} count
         * @param {string} prevSnapshot
         * @param {IPageBlobGetPageRangesDiffOptions} [options]
         * @returns {Promise<Models.PageBlobGetPageRangesDiffResponse>}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.getPageRangesDiff = function (aborter, offset, count, prevSnapshot, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    return [2 /*return*/, this.pageBlobContext.getPageRangesDiff({
                            abortSignal: aborter,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions,
                            prevsnapshot: prevSnapshot,
                            range: rangeToString({ offset: offset, count: count })
                        })];
                });
            });
        };
        /**
         * Resizes the page blob to the specified size (which must be a multiple of 512).
         * @see https://docs.microsoft.com/rest/api/storageservices/set-blob-properties
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {number} size
         * @param {IPageBlobResizeOptions} [options]
         * @returns {Promise<Models.PageBlobResizeResponse>}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.resize = function (aborter, size, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    return [2 /*return*/, this.pageBlobContext.resize(size, {
                            abortSignal: aborter,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * Sets a page blob's sequence number.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/set-blob-properties
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {Models.SequenceNumberActionType} sequenceNumberAction
         * @param {number} [sequenceNumber] Required if sequenceNumberAction is max or update
         * @param {IPageBlobUpdateSequenceNumberOptions} [options]
         * @returns {Promise<Models.PageBlobUpdateSequenceNumberResponse>}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.updateSequenceNumber = function (aborter, sequenceNumberAction, sequenceNumber, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    options.accessConditions = options.accessConditions || {};
                    return [2 /*return*/, this.pageBlobContext.updateSequenceNumber(sequenceNumberAction, {
                            abortSignal: aborter,
                            blobSequenceNumber: sequenceNumber,
                            leaseAccessConditions: options.accessConditions.leaseAccessConditions,
                            modifiedAccessConditions: options.accessConditions.modifiedAccessConditions
                        })];
                });
            });
        };
        /**
         * Begins an operation to start an incremental copy from one page blob's snapshot to this page blob.
         * The snapshot is copied such that only the differential changes between the previously
         * copied snapshot are transferred to the destination.
         * The copied snapshots are complete copies of the original snapshot and can be read or copied from as usual.
         * @see https://docs.microsoft.com/rest/api/storageservices/incremental-copy-blob
         * @see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/incremental-snapshots
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} copySource Specifies the name of the source page blob snapshot. For example,
         *                            https://myaccount.blob.core.windows.net/mycontainer/myblob?snapshot=<DateTime>
         * @param {IPageBlobStartCopyIncrementalOptions} [options]
         * @returns {Promise<Models.PageBlobCopyIncrementalResponse>}
         * @memberof PageBlobURL
         */
        PageBlobURL.prototype.startCopyIncremental = function (aborter, copySource, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.pageBlobContext.copyIncremental(copySource, {
                            abortSignal: aborter,
                            modifiedAccessConditions: options.modifiedAccessConditions
                        })];
                });
            });
        };
        return PageBlobURL;
    }(BlobURL));

    /**
     * Convert a Browser Blob object into string.
     *
     * @export
     * @param {Blob} blob
     * @returns {Promise<ArrayBuffer>}
     */
    function blobToString(blob) {
        return __awaiter(this, void 0, void 0, function () {
            var fileReader;
            return __generator(this, function (_a) {
                fileReader = new FileReader();
                return [2 /*return*/, new Promise(function (resolve, reject) {
                        fileReader.onloadend = function (ev) {
                            resolve(ev.target.result);
                        };
                        fileReader.onerror = reject;
                        fileReader.readAsText(blob);
                    })];
            });
        });
    }

    function getBodyAsText(batchResponse) {
        return __awaiter(this, void 0, void 0, function () {
            var blob;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, batchResponse.blobBody];
                    case 1:
                        blob = (_a.sent());
                        return [4 /*yield*/, blobToString(blob)];
                    case 2: return [2 /*return*/, _a.sent()];
                }
            });
        });
    }
    function utf8ByteLength(str) {
        return new Blob([str]).size;
    }

    var HTTP_HEADER_DELIMITER = ": ";
    var SPACE_DELIMITER = " ";
    var NOT_FOUND = -1;
    /**
     * Util class for parsing batch response.
     */
    var BatchResponseParser = /** @class */ (function () {
        function BatchResponseParser(batchResponse, subRequests) {
            if (!batchResponse || !batchResponse.contentType) {
                // In special case(reported), server may return invalid content-type which could not be parsed.
                throw new RangeError("batchResponse is malformed or doesn't contain valid content-type.");
            }
            if (!subRequests || subRequests.size === 0) {
                // This should be prevent during coding.
                throw new RangeError("Invalid state: subRequests is not provided or size is 0.");
            }
            this.batchResponse = batchResponse;
            this.subRequests = subRequests;
            this.responseBatchBoundary = this.batchResponse.contentType.split("=")[1];
            this.perResponsePrefix = "--" + this.responseBatchBoundary + HTTP_LINE_ENDING;
            this.batchResponseEnding = "--" + this.responseBatchBoundary + "--";
        }
        // For example of response, please refer to https://docs.microsoft.com/en-us/rest/api/storageservices/blob-batch#response
        BatchResponseParser.prototype.parseBatchResponse = function () {
            return __awaiter(this, void 0, void 0, function () {
                var responseBodyAsText, subResponses, subResponseCount, deserializedSubResponses, subResponsesSucceededCount, subResponsesFailedCount, index, subResponse, deserializedSubResponse, responseLines, subRespHeaderStartFound, subRespHeaderEndFound, subRespFailed, contentId, _i, responseLines_1, responseLine, tokens, tokens;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            // When logic reach here, suppose batch request has already succeeded with 202, so we can further parse
                            // sub request's response.
                            if (this.batchResponse._response.status != HTTPURLConnection.HTTP_ACCEPTED) {
                                throw new Error("Invalid state: batch request failed with status: '" + this.batchResponse._response.status + "'.");
                            }
                            return [4 /*yield*/, getBodyAsText(this.batchResponse)];
                        case 1:
                            responseBodyAsText = _a.sent();
                            subResponses = responseBodyAsText
                                .split(this.batchResponseEnding)[0] // string after ending is useless
                                .split(this.perResponsePrefix)
                                .slice(1);
                            subResponseCount = subResponses.length;
                            // Defensive coding in case of potential error parsing.
                            // Note: subResponseCount == 1 is special case where sub request is invalid.
                            // We try to prevent such cases through early validation, e.g. validate sub request count >= 1.
                            // While in unexpected sub request invalid case, we allow sub response to be parsed and return to user.
                            if (subResponseCount != this.subRequests.size && subResponseCount != 1) {
                                throw new Error("Invalid state: sub responses' count is not equal to sub requests' count.");
                            }
                            deserializedSubResponses = new Array(subResponseCount);
                            subResponsesSucceededCount = 0;
                            subResponsesFailedCount = 0;
                            // Parse sub subResponses.
                            for (index = 0; index < subResponseCount; index++) {
                                subResponse = subResponses[index];
                                deserializedSubResponses[index] = {};
                                deserializedSubResponse = deserializedSubResponses[index];
                                deserializedSubResponse.headers = new HttpHeaders();
                                responseLines = subResponse.split("" + HTTP_LINE_ENDING);
                                subRespHeaderStartFound = false;
                                subRespHeaderEndFound = false;
                                subRespFailed = false;
                                contentId = NOT_FOUND;
                                for (_i = 0, responseLines_1 = responseLines; _i < responseLines_1.length; _i++) {
                                    responseLine = responseLines_1[_i];
                                    if (!subRespHeaderStartFound) {
                                        // Convention line to indicate content ID
                                        if (responseLine.startsWith(HeaderConstants.CONTENT_ID)) {
                                            contentId = parseInt(responseLine.split(HTTP_HEADER_DELIMITER)[1]);
                                        }
                                        // Http version line with status code indicates the start of sub request's response.
                                        // Example: HTTP/1.1 202 Accepted 
                                        if (responseLine.startsWith(HTTP_VERSION_1_1)) {
                                            subRespHeaderStartFound = true;
                                            tokens = responseLine.split(SPACE_DELIMITER);
                                            deserializedSubResponse.status = parseInt(tokens[1]);
                                            deserializedSubResponse.statusMessage = tokens.slice(2).join(SPACE_DELIMITER);
                                        }
                                        continue; // Skip convention headers not specifically for sub request i.e. Content-Type: application/http and Content-ID: *
                                    }
                                    if (responseLine.trim() === "") {
                                        // Sub response's header start line already found, and the first empty line indicates header end line found. 
                                        if (!subRespHeaderEndFound) {
                                            subRespHeaderEndFound = true;
                                        }
                                        continue; // Skip empty line
                                    }
                                    // Note: when code reach here, it indicates subRespHeaderStartFound == true
                                    if (!subRespHeaderEndFound) {
                                        if (responseLine.indexOf(HTTP_HEADER_DELIMITER) === -1) {
                                            // Defensive coding to prevent from missing valuable lines.
                                            throw new Error("Invalid state: find non-empty line '" + responseLine + "' without HTTP header delimiter '" + HTTP_HEADER_DELIMITER + "'.");
                                        }
                                        tokens = responseLine.split(HTTP_HEADER_DELIMITER);
                                        deserializedSubResponse.headers.set(tokens[0], tokens[1]);
                                        if (tokens[0] === HeaderConstants.X_MS_ERROR_CODE) {
                                            deserializedSubResponse.errorCode = tokens[1];
                                            subRespFailed = true;
                                        }
                                    }
                                    else {
                                        // Assemble body of sub response.
                                        if (!deserializedSubResponse.bodyAsText) {
                                            deserializedSubResponse.bodyAsText = "";
                                        }
                                        deserializedSubResponse.bodyAsText += responseLine;
                                    }
                                } // Inner for end
                                if (contentId != NOT_FOUND) {
                                    deserializedSubResponse._request = this.subRequests.get(contentId);
                                }
                                if (subRespFailed) {
                                    subResponsesFailedCount++;
                                }
                                else {
                                    subResponsesSucceededCount++;
                                }
                            }
                            return [2 /*return*/, {
                                    subResponses: deserializedSubResponses,
                                    subResponsesSucceededCount: subResponsesSucceededCount,
                                    subResponsesFailedCount: subResponsesFailedCount
                                }];
                    }
                });
            });
        };
        return BatchResponseParser;
    }());

    /**
     * A ServiceURL represents a URL to the Azure Storage Blob service allowing you
     * to manipulate blob containers.
     *
     * @export
     * @class ServiceURL
     * @extends {StorageURL}
     */
    var ServiceURL = /** @class */ (function (_super) {
        __extends(ServiceURL, _super);
        /**
         * Creates an instance of ServiceURL.
         *
         * @param {string} url A URL string pointing to Azure Storage blob service, such as
         *                     "https://myaccount.blob.core.windows.net". You can append a SAS
         *                     if using AnonymousCredential, such as "https://myaccount.blob.core.windows.net?sasString".
         * @param {Pipeline} pipeline Call StorageURL.newPipeline() to create a default
         *                            pipeline, or provide a customized pipeline.
         * @memberof ServiceURL
         */
        function ServiceURL(url, pipeline) {
            var _this = _super.call(this, url, pipeline) || this;
            _this.serviceContext = new Service(_this.storageClientContext);
            return _this;
        }
        /**
         * Creates a new ServiceURL object identical to the source but with the
         * specified request policy pipeline.
         *
         * @param {Pipeline} pipeline
         * @returns {ServiceURL}
         * @memberof ServiceURL
         */
        ServiceURL.prototype.withPipeline = function (pipeline) {
            return new ServiceURL(this.url, pipeline);
        };
        /**
         * Gets the properties of a storage account’s Blob service, including properties
         * for Storage Analytics and CORS (Cross-Origin Resource Sharing) rules.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/get-blob-service-properties}
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @returns {Promise<Models.ServiceGetPropertiesResponse>}
         * @memberof ServiceURL
         */
        ServiceURL.prototype.getProperties = function (aborter) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.serviceContext.getProperties({
                            abortSignal: aborter
                        })];
                });
            });
        };
        /**
         * Sets properties for a storage account’s Blob service endpoint, including properties
         * for Storage Analytics, CORS (Cross-Origin Resource Sharing) rules and soft delete settings.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/set-blob-service-properties}
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {Models.StorageServiceProperties} properties
         * @returns {Promise<Models.ServiceSetPropertiesResponse>}
         * @memberof ServiceURL
         */
        ServiceURL.prototype.setProperties = function (aborter, properties) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.serviceContext.setProperties(properties, {
                            abortSignal: aborter
                        })];
                });
            });
        };
        /**
         * Retrieves statistics related to replication for the Blob service. It is only
         * available on the secondary location endpoint when read-access geo-redundant
         * replication is enabled for the storage account.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/get-blob-service-stats}
         *
         *  @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @returns {Promise<Models.ServiceGetStatisticsResponse>}
         * @memberof ServiceURL
         */
        ServiceURL.prototype.getStatistics = function (aborter) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.serviceContext.getStatistics({
                            abortSignal: aborter
                        })];
                });
            });
        };
        /**
         * The Get Account Information operation returns the sku name and account kind
         * for the specified account.
         * The Get Account Information operation is available on service versions beginning
         * with version 2018-03-28.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/get-account-information
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @returns {Promise<Models.ServiceGetAccountInfoResponse>}
         * @memberof ServiceURL
         */
        ServiceURL.prototype.getAccountInfo = function (aborter) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.serviceContext.getAccountInfo({
                            abortSignal: aborter
                        })];
                });
            });
        };
        /**
         * Returns a list of the containers under the specified account.
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/list-containers2
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {string} [marker] A string value that identifies the portion of
         *                          the list of containers to be returned with the next listing operation. The
         *                          operation returns the NextMarker value within the response body if the
         *                          listing operation did not return all containers remaining to be listed
         *                          with the current page. The NextMarker value can be used as the value for
         *                          the marker parameter in a subsequent call to request the next page of list
         *                          items. The marker value is opaque to the client.
         * @param {IServiceListContainersSegmentOptions} [options]
         * @returns {Promise<Models.ServiceListContainersSegmentResponse>}
         * @memberof ServiceURL
         */
        ServiceURL.prototype.listContainersSegment = function (aborter, marker, options) {
            if (options === void 0) { options = {}; }
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    return [2 /*return*/, this.serviceContext.listContainersSegment(__assign({ abortSignal: aborter, marker: marker }, options))];
                });
            });
        };
        /**
         * ONLY AVAILABLE WHEN USING BEARER TOKEN AUTHENTICATION (TokenCredential).
         *
         * Retrieves a user delegation key for the Blob service. This is only a valid operation when using
         * bearer token authentication.
         *
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/get-user-delegation-key
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation
         * @param {Date} start      The start time for the user delegation SAS. Must be within 7 days of the current time
         * @param {Date} expiry     The end time for the user delegation SAS. Must be within 7 days of the current time
         * @returns {Promise<ServiceGetUserDelegationKeyResponse>}
         * @memberof ServiceURL
         */
        ServiceURL.prototype.getUserDelegationKey = function (aborter, start, expiry) {
            return __awaiter(this, void 0, void 0, function () {
                var response, userDelegationKey, res;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0: return [4 /*yield*/, this.serviceContext.getUserDelegationKey({
                                start: truncatedISO8061Date(start, false),
                                expiry: truncatedISO8061Date(expiry, false)
                            }, {
                                abortSignal: aborter
                            })];
                        case 1:
                            response = _a.sent();
                            userDelegationKey = {
                                signedOid: response.signedOid,
                                signedTid: response.signedTid,
                                signedStart: new Date(response.signedStart),
                                signedExpiry: new Date(response.signedExpiry),
                                signedService: response.signedService,
                                signedVersion: response.signedVersion,
                                value: response.value
                            };
                            res = __assign({ _response: response._response, requestId: response.requestId, clientRequestId: response.clientRequestId, version: response.version, date: response.date, errorCode: response.errorCode }, userDelegationKey);
                            return [2 /*return*/, res];
                    }
                });
            });
        };
        /**
         * Submit batch request which consists of multiple subrequests.
         *
         * @example
         * let batchDeleteRequest = new BatchDeleteRequest();
         * await batchDeleteRequest.addSubRequest(urlInString0, credential0);
         * await batchDeleteRequest.addSubRequest(urlInString1, credential1, {
         *  deleteSnapshots: "include"
         * });
         * const deleteBatchResp = await serviceURL.submitBatch(Aborter.none, batchDeleteRequest);
         * console.log(deleteBatchResp.subResponsesSucceededCount);
         *
         * @example
         * let batchSetTierRequest = new BatchSetTierRequest();
         * await batchSetTierRequest.addSubRequest(blockBlobURL0, "Cool");
         * await batchSetTierRequest.addSubRequest(blockBlobURL1, "Cool", {
         *  leaseAccessConditions: { leaseId: leaseId }
         * });
         * const setTierBatchResp = await serviceURL.submitBatch(Aborter.none, batchSetTierRequest);
         * console.log(setTierBatchResp.subResponsesSucceededCount);
         *
         * @see https://docs.microsoft.com/en-us/rest/api/storageservices/blob-batch
         *
         * @param {Aborter} aborter Create a new Aborter instance with Aborter.none or Aborter.timeout(),
         *                          goto documents of Aborter for more examples about request cancellation.
         * @param {BatchRequest} batchRequest Supported batch request: BatchDeleteRequest or BatchSetTierRequest.
         * @param {Models.ServiceSubmitBatchOptionalParams} [options]
         * @returns {Promise<ServiceSubmitBatchResponse>}
         * @memberof ServiceURL
         */
        ServiceURL.prototype.submitBatch = function (aborter, batchRequest, options) {
            return __awaiter(this, void 0, void 0, function () {
                var batchRequestBody, rawBatchResponse, batchResponseParser, responseSummary, res;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!batchRequest || batchRequest.getSubRequests().size == 0) {
                                throw new RangeError("Batch request should contain one or more sub requests.");
                            }
                            batchRequestBody = batchRequest.getHttpRequestBody();
                            return [4 /*yield*/, this.serviceContext.submitBatch(batchRequestBody, utf8ByteLength(batchRequestBody), batchRequest.getMultiPartContentType(), __assign({ abortSignal: aborter }, options))];
                        case 1:
                            rawBatchResponse = _a.sent();
                            batchResponseParser = new BatchResponseParser(rawBatchResponse, batchRequest.getSubRequests());
                            return [4 /*yield*/, batchResponseParser.parseBatchResponse()];
                        case 2:
                            responseSummary = _a.sent();
                            res = {
                                _response: rawBatchResponse._response,
                                contentType: rawBatchResponse.contentType,
                                errorCode: rawBatchResponse.errorCode,
                                requestId: rawBatchResponse.requestId,
                                clientRequestId: rawBatchResponse.clientRequestId,
                                version: rawBatchResponse.version,
                                subResponses: responseSummary.subResponses,
                                subResponsesSucceededCount: responseSummary.subResponsesSucceededCount,
                                subResponsesFailedCount: responseSummary.subResponsesFailedCount
                            };
                            return [2 /*return*/, res];
                    }
                });
            });
        };
        return ServiceURL;
    }(StorageURL));

    exports.Aborter = Aborter;
    exports.AnonymousCredential = AnonymousCredential;
    exports.AnonymousCredentialPolicy = AnonymousCredentialPolicy;
    exports.AppendBlobURL = AppendBlobURL;
    exports.BaseRequestPolicy = BaseRequestPolicy;
    exports.BatchDeleteRequest = BatchDeleteRequest;
    exports.BatchRequest = BatchRequest;
    exports.BatchSetTierRequest = BatchSetTierRequest;
    exports.BlobURL = BlobURL;
    exports.BlockBlobURL = BlockBlobURL;
    exports.BrowserPolicyFactory = BrowserPolicyFactory;
    exports.ContainerURL = ContainerURL;
    exports.Credential = Credential;
    exports.CredentialPolicy = CredentialPolicy;
    exports.HttpHeaders = HttpHeaders;
    exports.LoggingPolicyFactory = LoggingPolicyFactory;
    exports.Models = index;
    exports.PageBlobURL = PageBlobURL;
    exports.Pipeline = Pipeline;
    exports.RequestPolicyOptions = RequestPolicyOptions;
    exports.RestError = RestError;
    exports.RetryPolicyFactory = RetryPolicyFactory;
    exports.ServiceURL = ServiceURL;
    exports.StorageURL = StorageURL;
    exports.TelemetryPolicyFactory = TelemetryPolicyFactory;
    exports.TokenCredential = TokenCredential;
    exports.TokenCredentialPolicy = TokenCredentialPolicy;
    exports.UniqueRequestIDPolicyFactory = UniqueRequestIDPolicyFactory;
    exports.WebResource = WebResource;
    exports.deserializationPolicy = deserializationPolicy;
    exports.uploadBrowserDataToBlockBlob = uploadBrowserDataToBlockBlob;

    Object.defineProperty(exports, '__esModule', { value: true });

}));
//# sourceMappingURL=azure-storage-blob.js.map
