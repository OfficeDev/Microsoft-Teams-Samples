import * as microsoftTeams from "@microsoft/teams-js";

const Detail = () => {
    function validateForm() {
        var pollnfo = {
            title: document.getElementById("title").value,
            option1: document.getElementById("option1").value,
            option2: document.getElementById("option2").value
        }
        
        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.dialog.submit(pollnfo);
        });
        return true;
    }
    return (
        <div className="surface theme-light">
            <div id="PollForm">
                <table>
                    <tr>
                        <td>
                            <label>
                                <b> Title: </b>
                            </label>
                        </td>
                        <td>
                            <input type="text" placeholder="Add Title" id ="title"name="title" size="30"/>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <label>
                                <b> Choice 1: </b>
                            </label>
                        </td>
                        <td>
                            <input type="text" placeholder="Add choice 1" id="option1" name="option1" size="30"/>
                        </td>
                   </tr>
                   <tr>
                        <td>
                            <label>
                                <b> Choice 2: </b>
                            </label>
                        </td>
                        <td>
                            <input type="text" placeholder="Add choice 2" id="option2" name="option2" size="30"/>
                        </td>
                </tr>
                <tr>
                    <td></td>
                    <td>
                        <input type="button" class="btn btn-primary" value="Create" onClick={() =>validateForm()}/>
                    </td>
                </tr>
                </table>
            </div>
    </div>
    )
};
export default Detail;
