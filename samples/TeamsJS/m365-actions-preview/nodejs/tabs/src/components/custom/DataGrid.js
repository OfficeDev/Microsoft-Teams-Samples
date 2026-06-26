import * as React from "react";

import {
    Attach20Filled,
    ContactCard20Regular,
    Delete20Regular,
    Edit20Regular,
    Notepad20Regular
} from "@fluentui/react-icons";
import {
    Input,
    Label,
    Menu,
    MenuButton,
    MenuItem,
    MenuList,
    MenuPopover,
    MenuTrigger,
    Table,
    TableBody,
    TableCell,
    TableCellLayout,
    TableHeader,
    TableHeaderCell,
    TableRow
} from "@fluentui/react-components";

import Creator from './Creator';
import { ToDoAttachment } from './Attachments';

const columns = [
    { columnKey: "isCompleted", label: "IsCompleted", icon: <Notepad20Regular /> },
    { columnKey: "note", label: "Note", icon: <Notepad20Regular /> },
    { columnKey: "attachment", label: "Attachment", icon: <Attach20Filled /> },
    { columnKey: "createdBy", label: "Created By", icon: <ContactCard20Regular /> },
    { columnKey: "action", label: "Action", icon: <ContactCard20Regular /> },
];

export const DataGrid = (props) => {
    return (
        <Table arial-label="Default table">
            <TableHeader>
                <TableRow>
                    {columns.map((column) => (
                        <TableHeaderCell key={column.columnKey} >
                            {column.icon}{column.label}
                        </TableHeaderCell>
                    ))}
                </TableRow>
            </TableHeader>
            <TableBody>
                {props.dbItems.map((item, index) => (
                    <TableRow key={item.id}>
                        <TableCell>
                            <input
                                className="is-completed-input"
                                style={{ verticalAlign: 'middle' }}
                                type="checkbox" checked={item.isCompleted === 0 ? false : true}
                                onChange={(e) => {
                                    props.onCompletionStatusChange(item.id, index, e.currentTarget.checked)
                                }} />
                        </TableCell>
                        <TableCell>
                            <TableCellLayout style={{ width: "100%" }}>
                                {props.editId !== item.id &&
                                    <Label as="label" size="small" onDoubleClick={() => { props.onEdit(item.id) }} className={"text" + (item.isCompleted ? " is-completed" : "")}>{item.description}</Label>
                                }
                                {props.editId && props.editId === item.id &&
                                    <Input
                                        value={item.description}
                                        onChange={(e) =>
                                            props.handleInputChange(index, "description", e.target.value)
                                        }
                                        onKeyDown={(e) => {
                                            if (e.key === "Enter") {
                                                props.onUpdateItem(item.id, item.description);
                                                e.target.blur();
                                                props.onEdit(undefined);
                                            }
                                        }}
                                        onBlur={() => {
                                            props.onUpdateItem(item.id, item.description);
                                            props.onEdit(undefined);
                                        }}
                                        className={
                                            "text" +
                                            (item.isCompleted ? " is-completed" : "")
                                        }
                                    />
                                }
                            </TableCellLayout>
                        </TableCell>
                        <TableCell>
                            <TableCellLayout >
                                {props.teamsUserCredential && item.itemId && item.objectId &&
                                    <ToDoAttachment
                                        teamsUserCredential={props.teamsUserCredential}
                                        scope={props.scope}
                                        attachmentId={item.itemId}
                                        singleAttachment={true}
                                        action={false}
                                        userId={item.objectId}
                                    />
                                }
                                {!item.itemId && <div>No attachment found</div>}
                            </TableCellLayout>
                        </TableCell>
                        <TableCell>
                            <Creator userInfo={props.userInfo} />
                        </TableCell>
                        <TableCell>
                            <Menu>
                                <MenuTrigger disableButtonEnhancement>
                                    <MenuButton appearance="subtle" menuIcon={null}>
                                        ...
                                    </MenuButton>
                                </MenuTrigger>
                                <MenuPopover>
                                    <MenuList>
                                        <MenuItem icon={<Edit20Regular />} onClick={() => { props.onEdit(item.id) }}>
                                            Edit
                                        </MenuItem>
                                        <MenuItem icon={<Delete20Regular />} onClick={() => props.onDeleteItem(item.id)}>
                                            Delete
                                        </MenuItem>
                                    </MenuList>
                                </MenuPopover>
                            </Menu>
                        </TableCell>
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    );
};