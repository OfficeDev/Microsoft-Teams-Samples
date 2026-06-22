import { v4 as uuidv4 } from 'uuid';

export class Task {
    id: string;
    title: string | null;
    description: string | null;
    createdBy: any;
    assignedTo: any;
    createdAt: string;
    status: string;

    constructor(
        id?: string | null,
        title?: string | null,
        description?: string | null,
        createdBy?: any,
        assignedTo?: any,
        createdAt?: string | null,
        status?: string | null
    ) {
        this.id = id || uuidv4();
        this.title = title || null;
        this.description = description || null;
        this.createdBy = createdBy || null;
        this.assignedTo = assignedTo || null;
        this.createdAt = createdAt || new Date().toLocaleString();
        this.status = status || 'Created';
    }

    setStatus(status: string): void {
        this.status = status || 'Created';
    }

    fill(newFields: Record<string, any>): void {
        for (const field in newFields) {
            if (Object.prototype.hasOwnProperty.call(this, field) &&
                Object.prototype.hasOwnProperty.call(newFields, field)) {
                (this as any)[field] = newFields[field];
            }
        }
    }
}
