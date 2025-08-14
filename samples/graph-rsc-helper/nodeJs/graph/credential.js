const identity = require('@azure/identity');
function getCredential() {
	const tenantId = process.env.TenantId;
	const clientId = process.env.ClientId;
	const clientSecret = process.env.ClientSecret;
	const credential = new identity.ClientSecretCredential(tenantId, clientId, clientSecret);
	return credential;
}

var credential = getCredential();
module.exports = credential;