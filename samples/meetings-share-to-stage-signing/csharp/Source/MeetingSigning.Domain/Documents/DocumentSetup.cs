// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Microsoft.Teams.Samples.MeetingSigning.Domain.Documents
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IRepositories;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.IServices;
    using Microsoft.Teams.Samples.MeetingSigning.Domain.Models;

    /// <summary>
    /// DocumentSetup is an intermediary class between data repositories and the Document Service
    /// </summary>
    // TODO - roll up this class to document service if there isn't much utility.
    public class DocumentSetup : IDocumentSetup
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ISignatureRepository _signatureRepository;
        private readonly IUserRepository _userRepository;
        private readonly IViewerRepository _viewerRepository;
        private readonly IUserService _userService;
        private readonly ILogger<DocumentSetup> _logger;

        public DocumentSetup(IDocumentRepository documentRepository,
            ISignatureRepository signatureRepository,
            IUserRepository userRepository,
            IViewerRepository viewerRepository,
            IUserService userService,
            ILogger<DocumentSetup> logger)
        {
            _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
            _signatureRepository = signatureRepository ?? throw new ArgumentNullException(nameof(signatureRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _viewerRepository = viewerRepository ?? throw new ArgumentNullException(nameof(viewerRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// AddDocumentAsync takes POST document API parameters and calls the data repositories required.
        /// </summary>
        /// <param name="documentDetails"></param>
        /// <param name="ownerId">AADId of the owner</param>
        /// <returns>Document created</returns>
        public async Task<Document> AddDocumentAsync(DocumentInput documentDetails, string ownerId)
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
                IList<Viewer>? viewers = await this.AddViewerAsync(viewersSet);

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
            try
            {
                ISet<User> users = new HashSet<User>(viewersSet);
                users.UnionWith(signersSet);

                foreach (User? user in users)
                {
                    if (!await _userRepository.UserExists(user.UserId))
                    {
                        await _userRepository.AddUser(await _userService.GetUser(user.UserId));
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
                foreach (User? signer in signersSet)
                {
                    var user = await this._userRepository.GetUser(signer.UserId);
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
        private async Task<IList<Viewer>> AddViewerAsync(ISet<User> viewersSet)
        {
            try
            {
                var viewers = new List<Viewer>();
                foreach (User? viewer in  viewersSet)
                {
                    User? user = await this._userRepository.GetUser(viewer.UserId);
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
