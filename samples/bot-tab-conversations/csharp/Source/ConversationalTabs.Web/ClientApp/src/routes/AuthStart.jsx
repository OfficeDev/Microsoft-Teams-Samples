import * as microsoftTeams from '@microsoft/teams-js';

/**
 * Component rendered when starting an auth prompt.
 * It gathers the required information before calling AAD
 */
export function AuthStart() {
  microsoftTeams.initialize();

  // Get the tab context, and use the information to navigate to Azure AD login page
  microsoftTeams.getContext(async function (context) {
    // Generate random state string and store it, so we can verify it in the callback
    let state = guid();
    localStorage.setItem('auth-state', state);
    localStorage.removeItem('codeVerifier');

    let tenantId = context['tid']; // Tenant ID of the logged in user
    let clientId = process.env.REACT_APP_AAD_CLIENT_ID;

    const queryParams = {
      tenant: tenantId,
      client_id: clientId,
      response_type: 'code',
      scope: 'https://graph.microsoft.com/.default',
      redirect_uri: `${window.location.origin}/auth-end`,
      nonce: guid(),
      state: state,
      login_hint: context.loginHint,
      prompt: 'consent',
    };

    let authorizeEndpoint = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/authorize?${toQueryString(
      queryParams,
    )}`;
    window.location.assign(authorizeEndpoint);
  });

  // Build query string from map of query parameter
  function toQueryString(queryParams) {
    let encodedQueryParams = [];
    for (let key in queryParams) {
      encodedQueryParams.push(key + '=' + encodeURIComponent(queryParams[key]));
    }
    return encodedQueryParams.join('&');
  }

  // Converts decimal to hex equivalent
  // (From ADAL.js: https://github.com/AzureAD/azure-activedirectory-library-for-js/blob/dev/lib/adal.js)
  function decimalToHex(number) {
    var hex = number.toString(16);
    while (hex.length < 2) {
      hex = '0' + hex;
    }
    return hex;
  }

  // Generates RFC4122 version 4 guid (128 bits)
  // (From ADAL.js: https://github.com/AzureAD/azure-activedirectory-library-for-js/blob/dev/lib/adal.js)
  function guid() {
    // RFC4122: The version 4 UUID is meant for generating UUIDs from truly-random or
    // pseudo-random numbers.
    // The algorithm is as follows:
    //     Set the two most significant bits (bits 6 and 7) of the
    //        clock_seq_hi_and_reserved to zero and one, respectively.
    //     Set the four most significant bits (bits 12 through 15) of the
    //        time_hi_and_version field to the 4-bit version number from
    //        Section 4.1.3. Version4
    //     Set all the other bits to randomly (or pseudo-randomly) chosen
    //     values.
    // UUID                   = time-low "-" time-mid "-"time-high-and-version "-"clock-seq-reserved and low(2hexOctet)"-" node
    // time-low               = 4hexOctet
    // time-mid               = 2hexOctet
    // time-high-and-version  = 2hexOctet
    // clock-seq-and-reserved = hexOctet:
    // clock-seq-low          = hexOctet
    // node                   = 6hexOctet
    // Format: xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx
    // y could be 1000, 1001, 1010, 1011 since most significant two bits needs to be 10
    // y values are 8, 9, A, B
    var cryptoObj = window.crypto || window.msCrypto; // for IE 11
    if (cryptoObj && cryptoObj.getRandomValues) {
      var buffer = new Uint8Array(16);
      cryptoObj.getRandomValues(buffer);
      //buffer[6] and buffer[7] represents the time_hi_and_version field. We will set the four most significant bits (4 through 7) of buffer[6] to represent decimal number 4 (UUID version number).
      buffer[6] |= 0x40; //buffer[6] | 01000000 will set the 6 bit to 1.
      buffer[6] &= 0x4f; //buffer[6] & 01001111 will set the 4, 5, and 7 bit to 0 such that bits 4-7 == 0100 = "4".
      //buffer[8] represents the clock_seq_hi_and_reserved field. We will set the two most significant bits (6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively.
      buffer[8] |= 0x80; //buffer[8] | 10000000 will set the 7 bit to 1.
      buffer[8] &= 0xbf; //buffer[8] & 10111111 will set the 6 bit to 0.
      return (
        decimalToHex(buffer[0]) +
        decimalToHex(buffer[1]) +
        decimalToHex(buffer[2]) +
        decimalToHex(buffer[3]) +
        '-' +
        decimalToHex(buffer[4]) +
        decimalToHex(buffer[5]) +
        '-' +
        decimalToHex(buffer[6]) +
        decimalToHex(buffer[7]) +
        '-' +
        decimalToHex(buffer[8]) +
        decimalToHex(buffer[9]) +
        '-' +
        decimalToHex(buffer[10]) +
        decimalToHex(buffer[11]) +
        decimalToHex(buffer[12]) +
        decimalToHex(buffer[13]) +
        decimalToHex(buffer[14]) +
        decimalToHex(buffer[15])
      );
    } else {
      var guidHolder = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx';
      var hex = '0123456789abcdef';
      var r = 0;
      var guidResponse = '';
      for (var i = 0; i < 36; i++) {
        if (guidHolder[i] !== '-' && guidHolder[i] !== '4') {
          // each x and y needs to be random
          r = (Math.random() * 16) | 0;
        }
        if (guidHolder[i] === 'x') {
          guidResponse += hex[r];
        } else if (guidHolder[i] === 'y') {
          // clock-seq-and-reserved first hex is filtered and remaining hex values are random
          r &= 0x3; // bit and with 0011 to set pos 2 to zero ?0??
          r |= 0x8; // set pos 3 to 1 as 1???
          guidResponse += hex[r];
        } else {
          guidResponse += guidHolder[i];
        }
      }
      return guidResponse;
    }
  }

  return (
    <>
      <p>
        To complete that request you need to authorize us to access your
        Microsoft Graph data.
      </p>
      <p>Redirecting...</p>
    </>
  );
}
