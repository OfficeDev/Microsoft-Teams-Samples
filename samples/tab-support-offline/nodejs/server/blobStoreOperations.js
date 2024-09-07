require('dotenv').config();
const { DefaultAzureCredential } = require("@azure/identity");
const { BlobServiceClient } = require("@azure/storage-blob");

// Replace these values with your Azure Storage account details
const accountName = 'supportricketblob';
const containerName = 'aircraft-issue-list';

const blobServiceClient = new BlobServiceClient(`https://${accountName}.blob.core.windows.net`, new DefaultAzureCredential());

async function getIssues(pageNumber = 1, pageSize = 20) {
    const containerClient = blobServiceClient.getContainerClient(containerName);
  
    const listBlobsResponse = await containerClient.listBlobsFlat();
    const allIssues = [];
  
    for await (const blob of listBlobsResponse) {
      const blobClient = containerClient.getBlobClient(blob.name);
      const downloadBlockBlobResponse = await blobClient.download();
      const content = await streamToString(downloadBlockBlobResponse.readableStreamBody);
      allIssues.push(JSON.parse(content));
    }
  
    // Sort issues by createdDate in descending order
    allIssues.sort((a, b) => new Date(b.createDate) - new Date(a.createDate));
  
    const startIndex = (pageNumber - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedIssues = allIssues.slice(startIndex, endIndex);
  
    return paginatedIssues;
}

async function getIssues(flightId, pageNumber = 1, pageSize = 20) {
  let containerClient = blobServiceClient.getContainerClient(containerName);

  let listBlobsResponse = await containerClient.listBlobsFlat();
  let allIssues = [];

  for await (const blob of listBlobsResponse) {
    const blobClient = containerClient.getBlobClient(blob.name);
    const downloadBlockBlobResponse = await blobClient.download();
    const content = await streamToString(downloadBlockBlobResponse.readableStreamBody);
    allIssues.push(JSON.parse(content));
  }

  allIssues = allIssues.filter(i => i.flightId === flightId);
  // Sort issues by createdDate in descending order
  allIssues.sort((a, b) => new Date(b.createDate) - new Date(a.createDate));

  const startIndex = (pageNumber - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const paginatedIssues = allIssues.slice(startIndex, endIndex);

  return paginatedIssues;
}

async function createIssue(issue) {
  try {
    const containerClient = blobServiceClient.getContainerClient(containerName);
    const blobName = `issue${issue.uid}`;
    const blockBlobClient = containerClient.getBlockBlobClient(blobName);
  
    const content = JSON.stringify(issue);
  
    await blockBlobClient.upload(content, content.length);
    return getIssue(issue.uid);  
  } catch (error) {
    console.log(error);
    throw error;
  }
}

async function getIssue(issueId) {
  const containerClient = blobServiceClient.getContainerClient(containerName);
  const blobName = `issue${issueId}`;
  const blockBlobClient = containerClient.getBlockBlobClient(blobName);

  const downloadBlockBlobResponse = await blockBlobClient.download();
  const content = await streamToString(downloadBlockBlobResponse.readableStreamBody);

  return JSON.parse(content);
}

async function updateIssue(issueId, updatedIssue) {
  const existingIssue = await getIssue(issueId);

  // Perform your optimistic locking check here if needed

  // Update the existing issue with new data
  const mergedIssue = { ...existingIssue, ...updatedIssue };

  await createIssue(mergedIssue);
}

async function deleteIssue(issueId) {
  const containerClient = blobServiceClient.getContainerClient(containerName);
  const blobName = `issue${issueId}`;
  const blockBlobClient = containerClient.getBlockBlobClient(blobName);

  await blockBlobClient.delete();
}

// Helper function to convert stream to string
async function streamToString(readableStream) {
  return new Promise((resolve, reject) => {
    const chunks = [];
    readableStream.on('data', (data) => {
      chunks.push(data.toString());
    });
    readableStream.on('end', () => {
      resolve(chunks.join(''));
    });
    readableStream.on('error', reject);
  });
}

module.exports = { getIssues, createIssue, getIssue, updateIssue, deleteIssue };