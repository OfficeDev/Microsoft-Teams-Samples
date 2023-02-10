// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Documents
{
    using System.Net;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Exceptions;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IServices;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <inheritdoc/>
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ISignatureRepository _signatureRepository;
        private readonly IUserRepository _userRepository;
        private readonly IViewerRepository _viewerRepository;
        private readonly IUserService _userService;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(IDocumentRepository documentRepository,
            ISignatureRepository signatureRepository,
            IUserRepository userRepository,
            IViewerRepository viewerRepository,
            IUserService userService,
            ILogger<DocumentService> logger)
        {
            _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
            _signatureRepository = signatureRepository ?? throw new ArgumentNullException(nameof(signatureRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _viewerRepository = viewerRepository ?? throw new ArgumentNullException(nameof(viewerRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<Document> CreateDocumentAsync(DocumentInput documentDetails, string ownerId)
        {
            _logger.LogInformation("About to create a document");
            Document document = await this.AddDocumentAsync(documentDetails, ownerId);
            _logger.LogInformation("Completed document creation");
            return document;
        }

        /// <inheritdoc/>
        public async Task<Document> GetDocumentAsync(Guid documentId)
        {
            Document document = await _documentRepository.GetDocument(documentId).ConfigureAwait(false);
            return document;
        }

        /// <inheritdoc/>
        public async Task<IList<Document>> GetDocumentsAsync(string ownerId)
        {
            IList<Document> documents = await _documentRepository.GetDocuments(ownerId).ConfigureAwait(false);
            return documents;
        }

        /// <inheritdoc/>
        public async Task<Signature> SignDocumentAsync(Guid id, Signature signatureAttempt)
        {
            signatureAttempt.SignedDateTime = DateTime.UtcNow;
            signatureAttempt.IsSigned = signatureAttempt.Text != null;

            Document document = await _documentRepository.GetDocument(id);

            if (document.Signatures.Any(s => s.Id == signatureAttempt.Id && s.Signer.UserId == signatureAttempt.Signer.UserId))
            {
                return await _signatureRepository.UpdateSignature(signatureAttempt);
            }
            else
            {
                throw new ApiException(HttpStatusCode.Forbidden, ErrorCode.Forbidden, "Signature is not valid for this Document.");
            }
        }

        /// <summary>
        /// AddDocumentAsync takes POST document API parameters and calls the data repositories required.
        /// </summary>
        /// <param name="documentDetails"></param>
        /// <param name="ownerId">AADId of the owner</param>
        /// <returns>Document created</returns>
        private async Task<Document> AddDocumentAsync(DocumentInput documentDetails, string ownerId)
        {
            try
            {
                var viewersSet = new HashSet<User>(documentDetails.Viewers);
                var signersSet = new HashSet<User>(documentDetails.Signers);

                //Add Users
                await this.AddUsersAsync(viewersSet, signersSet);

                //Add Signatures
                IList<Signature>? signatures = await this.AddSignaturesAsync(signersSet);

                //Add Viewers
                IList<Viewer>? viewers = await this.AddViewersAsync(viewersSet);

                var newDocument = new Document
                {
                    DocumentType = documentDetails.DocumentType,
                    Signatures = signatures,
                    OwnerId = ownerId,
                    Viewers = viewers,
                };
                Document? createdDocument = await this._documentRepository.CreateDocument(newDocument);

                return createdDocument;
            }
            catch (Exception ex)
            {
                this._logger.LogError("Failed to create document with exception", ex.Message);
                throw;
            }

        }

        /// <summary>
        /// AddUsersAsync calls the User repository to insert users from viewers and signers.
        /// </summary>
        /// <param name="viewersSet"></param>
        /// <param name="signersSet"></param>
        /// <returns>Task</returns>
        private async Task AddUsersAsync(ISet<User> viewersSet, ISet<User> signersSet)
        {
            ISet<User> users = new HashSet<User>(viewersSet);
            users.UnionWith(signersSet);

            // Check if any users will fail to be added because they are missing UserId or Email.
            // This prevents some users in a request being added successfully, and some failing
            if (users.Any(u => u.UserId == null && u.Email == null))
            {
                _logger.LogError("Unable to add all users, as at least one was missing a UserId or Email.");
                throw new ApiException(HttpStatusCode.BadRequest, ErrorCode.InvalidOperation, "Unable to add all users, as at least one was missing a UserId or Email.");
            }

            try
            {
                foreach (User user in users)
                {
                    if (!await _userRepository.UserExists(user.Id))
                    {
                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            await _userRepository.AddUser(new User { Name = user.Email, UserId = null, Email = user.Email });
                        }
                        else
                        {
                            await _userRepository.AddUser(await _userService.GetUser(user.UserId));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add users with exception", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// AddSignaturesAsync creates a Signature object and calls the Signature repository.
        /// </summary>
        /// <param name="documentDetails"></param>
        /// <returns>List of Signatures created</returns>
        private async Task<IList<Signature>> AddSignaturesAsync(ISet<User> signersSet)
        {
            try
            {
                var signatures = new List<Signature>();
                foreach (User signer in signersSet)
                {
                    var user = await this._userRepository.GetUser(signer.Id);
                    var signature = new Signature
                    {
                        Signer = user,
                        SignedDateTime = default,
                        Text = null,
                        IsSigned = false
                    };

                    Signature? addedSignature = await _signatureRepository.AddSignature(signature);
                    signatures.Add(addedSignature);
                }
                return signatures;
            }
            catch (Exception ex)
            {
                this._logger.LogError("Failed to add signatures while creating a document", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// AddViewerAsync creates a Viewer object and calls the Viewer repository.
        /// </summary>
        /// <param name="viewersSet"></param>
        /// <returns>List of Viewers created</returns>
        private async Task<IList<Viewer>> AddViewersAsync(ISet<User> viewersSet)
        {
            try
            {
                var viewers = new List<Viewer>();
                foreach (User viewer in viewersSet)
                {
                    User user = await this._userRepository.GetUser(viewer.Id);
                    var newViewer = new Viewer
                    {
                        Observer = user
                    };
                    viewers.Add(await this._viewerRepository.AddViewer(newViewer));
                }
                return viewers;
            }
            catch (Exception ex)
            {
                this._logger.LogError("Failed to add viewers with an exception", ex.Message);
                throw;
            }
        }
    }
}
