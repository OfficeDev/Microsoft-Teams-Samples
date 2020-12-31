// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

import * as assert from "assert";
import * as async from "async";
import * as builder from "botbuilder";
import * as mongodb from "mongodb";
import { IBotExtendedStorage } from "./BotExtendedStorage";
import * as logger from "winston";

// tslint:disable-next-line:variable-name
const Fields = {
    userData: "userData",
    conversationData: "conversationData",
    privateConversationData: "privateConversationData",
};

/** Replacable storage system used by UniversalBot. */
export class MongoDbBotStorage implements IBotExtendedStorage {

    private initializePromise: Promise<void>;
    private mongoDb: mongodb.Db;
    private botStateCollection: mongodb.Collection;

    constructor(
        private collectionName: string,
        private connectionString: string) {
    }

    // Reads in data from storage
    public getData(context: builder.IBotStorageContext, callback: (err: Error, data: builder.IBotStorageData) => void): void {
        this.initialize().then(() => {
            // Build list of read commands
            let list: any[] = [];
            if (context.userId) {
                if (context.persistUserData) {
                    // Read userData
                    list.push({
                        id: this.getUserDataId(context),
                        field: Fields.userData,
                    });
                }
                if (context.conversationId) {
                    // Read privateConversationData
                    list.push({
                        id: this.getPrivateConversationDataId(context),
                        field: Fields.privateConversationData,
                    });
                }
            }
            if (context.persistConversationData && context.conversationId) {
                // Read conversationData
                list.push({
                    id: this.getConversationDataId(context),
                    field: Fields.conversationData,
                });
            }

            // Execute reads in parallel
            let data: builder.IBotStorageData = {};
            async.each(list, (entry, cb) => {
                let filter = { key: entry.id };
                this.botStateCollection.findOne(filter, (error: any, entity: any) => {
                    if (!error) {
                        if (entity) {
                            let botData = entity.data || {};
                            try {
                                (data as any)[entry.field] = botData != null ? botData : null;
                            } catch (e) {
                                error = e;
                            }
                            cb(error);
                        } else {
                            (data as any)[entry.field] = null;
                            cb(error);
                        }
                    } else {
                        cb(error);
                    }
                });
            }, (err) => {
                if (!err) {
                    callback(null, data);
                } else {
                    let m = err.toString();
                    callback(err instanceof Error ? err : new Error(m), null);
                }
            });
        }).catch(err => callback(err, null));
    }

    // Writes out data from storage
    public saveData(context: builder.IBotStorageContext, data: builder.IBotStorageData, callback?: (err: Error) => void): void {
        // We initialize on every call, but only block on the first call. The reason for this is that we can't run asynchronous initialization in the class ctor
        this.initialize().then(() => {
            // Build list of write commands
            let list: any[] = [];
            if (context.userId) {
                if (context.persistUserData) {
                    // Write userData
                    list.push({
                        id: this.getUserDataId(context),
                        userId: context.userId,
                        field: Fields.userData,
                        botData: data.userData,
                    });
                }
                if (context.conversationId) {
                    // Write privateConversationData
                    list.push({
                        id: this.getPrivateConversationDataId(context),
                        userId: context.userId,
                        conversationId: context.conversationId,
                        field: Fields.privateConversationData,
                        botData: data.privateConversationData,
                    });
                }
            }
            if (context.persistConversationData && context.conversationId) {
                // Write conversationData
                list.push({
                    id: this.getConversationDataId(context),
                    conversationId: context.conversationId,
                    field: Fields.conversationData,
                    botData: data.conversationData,
                });
            }

            // Execute writes in parallel
            async.each(list, (entry, errorCallback) => {
                let filter = { key: entry.id };
                let document: any = {
                    key: entry.id,
                    field: entry.field,
                    data: entry.botData,
                    lastUpdate: new Date().valueOf(),
                };
                // Tag document with user id so we can find all user data later
                if (entry.userId) {
                    document.userId = entry.userId;
                }
                this.botStateCollection.updateOne(filter, document, { upsert: true }, err => errorCallback(err));
            }, (err) => {
                if (callback) {
                    if (!err) {
                        callback(null);
                    } else {
                        let m = err.toString();
                        callback(err instanceof Error ? err : new Error(m));
                    }
                }
            });
        }, (err) => callback(err));
    }

    // Lookup user data by AAD object id
    public async getUserDataByAadObjectIdAsync(aadObjectId: string): Promise<any> {
        await this.initialize();

        let filter = {
            field: Fields.userData,
            "data.aadObjectId": aadObjectId,
        };
        let entity = await this.botStateCollection.findOne(filter);
        if (entity) {
            return entity.data || {};
        } else {
            return null;
        }
    }

    public getAAdObjectId(userData: any): string {
        // This implementation sets the AAD object ID directly on userData
        return userData.aadObjectId;
    }

    public setAAdObjectId(userData: any, aadObjectId: string): void {
        // This implementation sets the AAD object ID directly on userData
        userData.aadObjectId = aadObjectId;
    }

    // Returns a promise that is resolved when this instance is initialized
    private initialize(): Promise<void> {
        if (!this.initializePromise) {
            this.initializePromise = this.initializeWorker();
        }
        return this.initializePromise;
    }

    // Initialize this instance
    private async initializeWorker(): Promise<void> {
        if (!this.mongoDb) {
            try {
                this.mongoDb = await mongodb.MongoClient.connect(this.connectionString);
                this.botStateCollection = await this.mongoDb.collection(this.collectionName);

                // Set up indexes
                await this.botStateCollection.createIndex({ key: 1 });
                await this.botStateCollection.createIndex({ lastUpdate: 1 });
            } catch (e) {
                logger.error(`Error initializing MongoDB: ${e.message}`, e);
                this.close();
                this.initializePromise = null;
            }
        }
    }

    // Close the connection to the database
    private close(): void {
        this.botStateCollection = null;
        if (this.mongoDb) {
            this.mongoDb.close();
            this.mongoDb = null;
        }
    }

    // Get id for user data documents
    private getUserDataId(context: builder.IBotStorageContext): string {
        assert(context.userId);
        return `user:${context.userId}`;
    }

    // Get id for conversation data documents
    private getConversationDataId(context: builder.IBotStorageContext): string {
        assert(context.conversationId);
        return `conversation:${context.conversationId}`;
    }

    // Get id for conversation data documents
    private getPrivateConversationDataId(context: builder.IBotStorageContext): string {
        assert(context.conversationId && context.userId);
        return `conversation:${context.conversationId}/user:${context.userId}`;
    }
}
