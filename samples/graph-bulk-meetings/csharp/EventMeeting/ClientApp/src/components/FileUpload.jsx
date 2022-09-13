import React, { Component } from 'react';
import "../style/style.css";
import { format } from 'date-fns';
import { OutTable, ExcelRenderer } from 'react-excel-renderer';
import { Col, Input, InputGroup, FormGroup, Label, Button, Fade, FormFeedback, Container, Card } from 'reactstrap';
import * as microsoftTeams from "@microsoft/teams-js";
import axios from "axios";
microsoftTeams.initialize();

class App extends Component {

    constructor(props) {
        super(props);
        this.state = {
            isOpen: false,
            dataLoaded: false,
            isFormInvalid: false,
            header: 0,
            blankrows: false,
            rows: null,
            cols: null
        }
        this.fileHandler = this.fileHandler.bind(this);
        this.toggle = this.toggle.bind(this);
        this.openFileBrowser = this.openFileBrowser.bind(this);
        this.renderFile = this.renderFile.bind(this);
        this.openNewPage = this.openNewPage.bind(this);
        this.fileInput = React.createRef();
    }
    renderFile = (fileObj) => {
        //just pass the fileObj as parameter
        ExcelRenderer(fileObj, (err, resp) => {
            if (err) {
                console.log(err);
            }
            else {
                this.setState({
                    dataLoaded: true,
                    cols: resp.cols,
                    rows: resp.rows
                });
                console.log(resp.cols, resp.rows);
            }
        });
    }

    fileHandler = (event) => {
        if (event.target.files.length) {
            let fileObj = event.target.files[0];
            let fileName = fileObj.name;


            //check for file extension and pass only if it is .xlsx and display error message otherwise
            if (fileName.slice(fileName.lastIndexOf('.') + 1) === "xlsx") {
                this.setState({
                    uploadedFileName: fileName,
                    isFormInvalid: false
                });
                this.renderFile(fileObj)
            }
            else {
                this.setState({
                    isFormInvalid: true,
                    uploadedFileName: ""
                })
            }
        }
    }

    // Handler when user click on create button.onCreateTeamsMeetingButtonClick
    onCreateTeamsMeetingButtonClick = (event) => {
        microsoftTeams.app.getContext().then(async (context) => {
            var excelrows = [];
            for (var i = 1; i < this.state.rows.length; i++) {
                var obj = {
                    TopicName: this.state.rows[i][0],
                    TrainerName: this.state.rows[i][1],
                    StartDate: this.state.rows[i][2],
                    EndDate: this.state.rows[i][3],
                    Timing: this.state.rows[i][4],
                    Participants: this.state.rows[i][5]
                }
                excelrows.push(obj);               
            }

            var response = await axios.post(`api/meeting/${context.user.id}`, excelrows);
            if (response.status === 201) {
                microsoftTeams.dialog.submit("Created successfully!");
            }
        });
    }

    toggle() {
        this.setState({
            isOpen: !this.state.isOpen
        });
    }

    openFileBrowser = () => {
        this.fileInput.current.click();
    }

    openNewPage = (chosenItem) => {
        const url = chosenItem === "github" ? "https://github.com/react-excel-renderer" : "https://medium.com/render-and-display-excel-sheets-on-webpage-using-react-js-af785a5db6a7";
        window.open(url, '_blank');
    }

    render() {
        return (
            <div>
                <div>
                </div>
                <Container>
                    <form>
                        <FormGroup row>
                            <Label for="exampleFile" xs={6} sm={4} lg={2} size="lg">Upload</Label>
                            <Col xs={4} sm={8} lg={10}>
                                <InputGroup>
                                    <InputGroup addonType="prepend">
                                        <Button color="info" style={{ color: "black", zIndex: 0 }} onClick={this.openFileBrowser.bind(this)}><i className="cui-file"></i> Browse&hellip;</Button>
                                        &nbsp;&nbsp; &nbsp;&nbsp; &nbsp;&nbsp;
                                        <input type="file" hidden onChange={this.fileHandler.bind(this)} ref={this.fileInput} onClick={(event) => { event.target.value = null }} style={{ marginLeft: 500 }} />

                                        <Button className="primary jumbotron-button" onClick={this.onCreateTeamsMeetingButtonClick.bind(this)} style={{ marginLeft: 250 }}>Create meetings</Button>
                                    </InputGroup>
                                    <Input type="text" className="form-control" value={this.state.uploadedFileName} readOnly invalid={this.state.isFormInvalid} />
                                    <FormFeedback>
                                        <Fade in={this.state.isFormInvalid} tag="h6" style={{ fontStyle: "italic" }}>
                                            Please select a .xlsx file only. !
                                        </Fade>
                                    </FormFeedback>
                                </InputGroup>
                            </Col>
                        </FormGroup>
                    </form>

                    {this.state.dataLoaded &&
                        <div>
                            <Card body outline color="secondary" className="restrict-card" >
                                <OutTable data={this.state.rows} columns={this.state.cols} tableClassName="ExcelTable2007" tableHeaderRowClass="heading" />
                            </Card>
                        </div>}
                </Container>

            </div>
        );
    }

}

export default App;