// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

package com.microsoft.bot.sample.teamsfileupload;

import com.microsoft.bot.builder.MessageFactory;
import com.microsoft.bot.builder.TurnContext;
import com.microsoft.bot.builder.teams.TeamsActivityHandler;
import com.microsoft.bot.connector.authentication.MicrosoftAppCredentials;
import com.microsoft.bot.integration.Configuration;
import com.microsoft.bot.schema.Activity;
import com.microsoft.bot.schema.Attachment;
import com.microsoft.bot.schema.ResultPair;
import com.microsoft.bot.schema.Serialization;
import com.microsoft.bot.schema.TextFormatTypes;
import com.microsoft.bot.schema.teams.FileConsentCard;
import com.microsoft.bot.schema.teams.FileConsentCardResponse;
import com.microsoft.bot.schema.teams.FileDownloadInfo;
import com.microsoft.bot.schema.teams.FileInfoCard;
import org.apache.commons.codec.binary.Base64;
import org.apache.commons.io.FileUtils;
import org.apache.commons.lang3.StringUtils;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.atomic.AtomicReference;

/**
 * This class implements the functionality of the Bot.
 *
 * <p>This is where application specific logic for interacting with the users would be
 * added.  For this sample, the {@link #onMessageActivity(TurnContext)} echos the text back to the
 * user.  The {@link #onMembersAdded(List, TurnContext)} will send a greeting to new conversation
 * participants.</p>
 */
public class TeamsFileUploadBot extends TeamsActivityHandler {
    // You can install this bot at any scope. You can @mention the bot and it will present you with the file prompt. You can accept and
    // the file will be uploaded, or you can decline and it won't.

    private static String microsoftAppId;
    private static String microsoftAppPassword;
    private static final String FILES_DIR = "files";
    private TurnContext context;

    public TeamsFileUploadBot(Configuration configuration) {
        microsoftAppId = configuration.getProperty("MicrosoftAppId");
        microsoftAppPassword = configuration.getProperty("MicrosoftAppPassword");
    }

    @Override
    protected CompletableFuture<Void> onMessageActivity(TurnContext turnContext) {
        if (messageWithDownload(turnContext.getActivity())) {
            Attachment attachment = turnContext.getActivity().getAttachments().get(0);
            return downloadAttachment(attachment)
                .thenCompose(result -> !result.result()
                    ? fileDownloadFailed(turnContext, result.value())
                    : fileDownloadCompleted(turnContext, attachment)
                );
        } else if (turnContext.getActivity().getAttachments() != null
                && turnContext.getActivity().getAttachments().get(0).getContentType().contains("image/*")) {
            // Inline image se.
            return processInlineImage(turnContext);
        } else {
            File filePath = new File("files", "teams-logo.png");
            return sendFileCard(turnContext, filePath.getName(), filePath.length());
        }
    }

    @Override
    protected CompletableFuture<Void> onTeamsFileConsentAccept(
        TurnContext turnContext,
        FileConsentCardResponse fileConsentCardResponse
    ) {
        return upload(fileConsentCardResponse)
            .thenCompose(result -> !result.result()
                ? fileUploadFailed(turnContext, result.value())
                : fileUploadCompleted(turnContext, fileConsentCardResponse)
            );
    }

    @Override
    protected CompletableFuture<Void> onTeamsFileConsentDecline(
        TurnContext turnContext,
        FileConsentCardResponse fileConsentCardResponse
    ) {
        Map<String, String> context = (Map<String, String>) fileConsentCardResponse.getContext();

        Activity reply = MessageFactory.text(String.format(
            "Declined. We won't upload file <b>{context[%s]}</b>.", context.get("filename"))
        );
        reply.setTextFormat(TextFormatTypes.XML);

        return turnContext.sendActivityBlind(reply);
    }

    private CompletableFuture<Void> sendFileCard(
        TurnContext turnContext, String filename, long filesize
    ) {
        Map<String, String> consentContext = new HashMap<>();
        consentContext.put("filename", filename);

        FileConsentCard fileCard = new FileConsentCard();
        fileCard.setDescription("This is the file I want to send you");
        fileCard.setSizeInBytes(filesize);
        fileCard.setAcceptContext(consentContext);
        fileCard.setDeclineContext(consentContext);

        Attachment asAttachment = new Attachment();
        asAttachment.setContent(fileCard);
        asAttachment.setContentType(FileConsentCard.CONTENT_TYPE);
        asAttachment.setName(filename);

        Activity reply = turnContext.getActivity().createReply();
        reply.setAttachments(Collections.singletonList(asAttachment));

        return turnContext.sendActivityBlind(reply);
    }

    private CompletableFuture<Void> fileUploadCompleted(
        TurnContext turnContext, FileConsentCardResponse fileConsentCardResponse
    ) {
        FileInfoCard downloadCard = new FileInfoCard();
        downloadCard.setUniqueId(fileConsentCardResponse.getUploadInfo().getUniqueId());
        downloadCard.setFileType(fileConsentCardResponse.getUploadInfo().getFileType());

        Attachment asAttachment = new Attachment();
        asAttachment.setContent(downloadCard);
        asAttachment.setContentType(FileInfoCard.CONTENT_TYPE);
        asAttachment.setName(fileConsentCardResponse.getUploadInfo().getName());
        asAttachment.setContentUrl(fileConsentCardResponse.getUploadInfo().getContentUrl());

        Activity reply = MessageFactory.text(
            String.format(
                "<b>File uploaded.</b> Your file <b>%s</b> is ready to download",
                fileConsentCardResponse.getUploadInfo().getName()
            )
        );
        reply.setTextFormat(TextFormatTypes.XML);
        reply.setAttachment(asAttachment);

        return turnContext.sendActivityBlind(reply);
    }

    private CompletableFuture<Void> fileUploadFailed(TurnContext turnContext, String error) {
        Activity reply = MessageFactory
            .text("<b>File upload failed.</b> Error: <pre>" + error + "</pre>");
        reply.setTextFormat(TextFormatTypes.XML);
        return turnContext.sendActivityBlind(reply);
    }

    private CompletableFuture<Void> fileDownloadCompleted(
        TurnContext turnContext, Attachment attachment
    ) {
        Activity reply = MessageFactory.text(String.format(
            "<b>%s</b> received and saved.",
            attachment.getName()
        ));
        reply.setTextFormat(TextFormatTypes.XML);

        return turnContext.sendActivityBlind(reply);
    }

    private CompletableFuture<Void> fileDownloadFailed(TurnContext turnContext, String error) {
        Activity reply = MessageFactory
            .text("<b>File download failed.</b> Error: <pre>" + error + "</pre>");
        reply.setTextFormat(TextFormatTypes.XML);
        return turnContext.sendActivityBlind(reply);
    }

    private boolean messageWithDownload(Activity activity) {
        boolean messageWithFileDownloadInfo = false;
        if (activity.getAttachments() != null
            && activity.getAttachments().size() > 0
        ) {
            messageWithFileDownloadInfo = StringUtils.equalsIgnoreCase(
                activity.getAttachments().get(0).getContentType(),
                FileDownloadInfo.CONTENT_TYPE
            );
        }
        return messageWithFileDownloadInfo;
    }


    private CompletableFuture<ResultPair<String>> upload(FileConsentCardResponse fileConsentCardResponse) {
        AtomicReference<ResultPair<String>> result = new AtomicReference<>();

        return CompletableFuture.runAsync(() -> {
            Map<String, String> context = (Map<String, String>) fileConsentCardResponse.getContext();
            File filePath = new File("files", context.get("filename"));
            HttpURLConnection connection = null;


            
                try {
                    URI uri = new URI(fileConsentCardResponse.getUploadInfo().getUploadUrl());
                    URL url = uri.toURL();
                    connection = (HttpURLConnection) url.openConnection();
                    connection.setRequestMethod("PUT");
                    connection.setDoOutput(true);
                    connection.setRequestProperty("Content-Type", "image/png");
                    connection.setRequestProperty("Content-Length", Long.toString(filePath.length()));
                    connection.setRequestProperty(
                        "Content-Range",
                        String.format("bytes 0-%d/%d", filePath.length() - 1, filePath.length())
                    );

                    try (FileInputStream fileStream = new FileInputStream(filePath);
                         OutputStream uploadStream = connection.getOutputStream()) {

                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = fileStream.read(buffer)) != -1) {
                            uploadStream.write(buffer, 0, bytesRead);
                        }
                    }

                    int responseCode = connection.getResponseCode();
                    if (responseCode == HttpURLConnection.HTTP_OK || responseCode == HttpURLConnection.HTTP_CREATED) {
                        result.set(new ResultPair<>(true, null));
                        
                    } else {
                        result.set(new ResultPair<>(false, "Failed with HTTP error code: " + responseCode));
                        
                        Thread.sleep(1000); // Simple backoff before retrying
                    }
                } catch (IOException e) {
                    result.set(new ResultPair<>(false, "IOException: " + e.getMessage()));
                    
                } catch (URISyntaxException e) {
                    result.set(new ResultPair<>(false, "URISyntaxException: " + e.getMessage()));
                 
                } catch (InterruptedException e) {
                    Thread.currentThread().interrupt(); // Restore interrupt status
                    result.set(new ResultPair<>(false, "InterruptedException: " + e.getMessage()));
                 
                } finally {
                    if (connection != null) {
                        connection.disconnect();
                    }
                }

            
        }).thenApply(aVoid -> result.get());
    }



    private CompletableFuture<Void> processInlineImage(TurnContext turnContext) {
        Attachment attachment = turnContext.getActivity().getAttachments().get(0);
        final HttpURLConnection[] connection = {null};
        // Get Bot's access token to fetch inline image.
        return new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword).getToken().thenApply(token -> {
            try {
                // Save the inline image to Files directory.
                String filePath = "files/imageFromUser.png";
                URI uri = new URI(attachment.getContentUrl());
                URL url = uri.toURL();
                connection[0] = (HttpURLConnection) url.openConnection();
                connection[0].setRequestProperty("Authorization", "Bearer " + token);
                try (
                    FileOutputStream fileStream = new FileOutputStream(filePath);
                    InputStream downloadStream = connection[0].getInputStream()
                ) {
                    byte[] buffer = new byte[4096];
                    int bytes_read;
                    while ((bytes_read = downloadStream.read(buffer)) != -1) {
                        fileStream.write(buffer, 0, bytes_read);
                    }
                    // Create reply with image.
                    byte[] fileSize = FileUtils.readFileToByteArray(new File(filePath));
                    Activity reply = MessageFactory.text(
                            String.format(
                                    "Attachment of %s type and size of %d bytes received",
                                    attachment.getContentType(),
                                    fileSize.length));
                    List<Attachment> attachments = new ArrayList<>();
                    attachments.add(getInlineAttachment());
                    reply.setAttachments(attachments);
                    return turnContext.sendActivity(reply);
                }
            } catch (Throwable t) {
            }  finally {
                if (connection[0] != null) {
                    connection[0].disconnect();
                }
            }
            return null;
        }).thenApply(aVoid -> null);
    }

    private static Attachment getInlineAttachment() {
        String imagePath = "files/imageFromUser.png";
        Attachment attachment = new Attachment();
        File file = new File(imagePath);
        try {
            String imageData = Base64.encodeBase64String(FileUtils.readFileToByteArray(file));
            attachment.setName("imageFromUser.png");
            attachment.setContentType("image/png");
            attachment.setContentUrl("data:image/png;base64," + imageData);
        } catch (IOException e) {
        }
        return attachment;
    }

    private CompletableFuture<ResultPair<String>> downloadAttachment(Attachment attachment) {
        AtomicReference<ResultPair<String>> result = new AtomicReference<>();

        return CompletableFuture.runAsync(() -> {
            FileDownloadInfo fileDownload = Serialization
                .getAs(attachment.getContent(), FileDownloadInfo.class);
            String filePath = "files/" + attachment.getName();

            HttpURLConnection connection = null;

            try {
                URI uri = new URI(fileDownload.getDownloadUrl());
                URL url = uri.toURL();
                connection = (HttpURLConnection) url.openConnection();
                connection.setDoInput(true);

                try (
                    FileOutputStream fileStream = new FileOutputStream(filePath);
                    InputStream downloadStream = connection.getInputStream()
                ) {
                    byte[] buffer = new byte[4096];
                    int bytes_read;
                    while ((bytes_read = downloadStream.read(buffer)) != -1) {
                        fileStream.write(buffer, 0, bytes_read);
                    }
                }

                result.set(new ResultPair<>(true, null));
            } catch (Throwable t) {
                result.set(new ResultPair<>(false, t.getLocalizedMessage()));
            } finally {
                if (connection != null) {
                    connection.disconnect();
                }
            }
        })
            .thenApply(aVoid -> result.get());
    }
}
