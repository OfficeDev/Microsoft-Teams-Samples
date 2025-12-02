const fs = require('fs');
const axios = require('axios');

// Generates a file name based on a sequence of existing files
const generateFileName = async (fileDir) => {
    const filenamePrefix = 'UserAttachment';
    const files = await fs.promises.readdir(fileDir); // Use promises API for readdir
    const filteredFiles = files
        .filter(f => f.includes(filenamePrefix))  // Filter files by prefix
        .map(f => parseInt(f.split(filenamePrefix)[1].split('.')[0]))  // Extract sequence numbers from filenames
        .filter(num => !isNaN(num));  // Ensure all numbers are valid
    const maxSeq = filteredFiles.length > 0 ? Math.max(...filteredFiles) : 0;  // Use spread operator for max calculation
    const filename = `${filenamePrefix}${maxSeq + 1}.png`;
    return filename;
};

// Downloads content from a URL and saves it to the specified file path
const writeFile = async (contentUrl, config, filePath) => {
    try {
        const response = await axios({ method: 'GET', url: contentUrl, ...config });
        return new Promise((resolve, reject) => {
            response.data
                .pipe(fs.createWriteStream(filePath))
                .once('finish', resolve)
                .once('error', reject);
        });
    } catch (error) {
        console.error('Error downloading the file:', error.message); // Better error logging
        throw new Error('Failed to download file');
    }
};

// Returns the size of a file
const getFileSize = async (filePath) => {
    try {
        const stats = await fs.promises.stat(filePath);  // Use promises API for stat
        return stats.size;
    } catch (error) {
        console.error('Error retrieving file size:', error.message);  // Better error logging
        throw new Error('Failed to retrieve file size');
    }
};

module.exports = {
    generateFileName,
    getFileSize,
    writeFile
};