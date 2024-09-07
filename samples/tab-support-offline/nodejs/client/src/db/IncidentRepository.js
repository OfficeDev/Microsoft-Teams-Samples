import IndexedDb from './IndexedDb'

class IncidentRepository {
    constructor() {
        this.dbName = "incidentdb";
        this.tableName = "userform";
        this.processedRecords = "userFormProcessed";
        this.db = new IndexedDb(this.dbName);
        this.channel = new BroadcastChannel('datasync');
        this.formPostUrl = "";
        this.db.createObjectStore([this.tableName, this.processedRecords]);
    }

    async getPendingRecordsCount() {
        await this.db.createObjectStore([this.tableName]);
        const keys = await this.db.getAllKeys(this.tableName);
        return keys.length;
    }

    async updateRecordById(id, updatedRecord) {
        await this.db.createObjectStore([this.tableName]);
        const record = await this.db.getValue(this.tableName, id);
        if (!record) {
            throw new Error(`Record with ID ${id} not found`);
        }
        const updatedRecordWithId = { ...updatedRecord, id };
        await this.db.putValue(this.tableName, updatedRecordWithId);
    }

    async getRecords() {
        await this.db.createObjectStore([this.tableName]);
        const records = await this.db.getAllValue(this.tableName);
        return records
    }

    async getFlightRecords(flightId) {
        await this.db.createObjectStore([this.tableName]);
        const records = await this.db.getAllValue(this.tableName);
        return records.filter(record => record.flightId === flightId);
    }

    async saveRecord(incidentRecord) {
        await this.db.createObjectStore([this.tableName]);
        this.db.putValue(this.tableName, incidentRecord);
    }
    

    async addProcessedRecord(incidentRecord) {
        await this.db.createObjectStore([this.processedRecords]);
        this.db.putValue(this.processedRecords, incidentRecord);
    }

    async getProcessedRecordsCount() {
        await this.db.createObjectStore([this.processedRecords]);
        const keys = await this.db.getAllKeys(this.processedRecords);
        return keys.length;
    }

    async deleteRecord(id) {
        await this.db.createObjectStore([this.tableName]);
        this.db.deleteValue(this.tableName, id);
    }
}

export default IncidentRepository;