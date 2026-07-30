"use strict";
const fs = require('fs');
const axios = require('axios');
// Download and Save Streams into File
const writeFile = async (contentUrl, filePath) => {
    const response = await axios({ method: 'GET', url: contentUrl, responseType: 'stream' });
    return await new Promise((resolve, reject) => response.data.pipe(fs.createWriteStream(filePath)).once('finish', resolve).once('error', reject));
};
module.exports = {
    writeFile
};
//# sourceMappingURL=fileService.js.map