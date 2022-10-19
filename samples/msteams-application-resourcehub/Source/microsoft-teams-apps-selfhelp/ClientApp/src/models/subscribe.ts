export default interface ISubscribe {
    partitionKey: string;
    RowKey: string;
    eTag:string;
    timestamp:Date;
    userId:string;
    tenantId:string;
    email:string;
    status:boolean;
    createdOn: Date;
    createdBy: string;
}