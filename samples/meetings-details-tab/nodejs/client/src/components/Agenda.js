import * as microsoftTeams from "@microsoft/teams-js";
const Agenda = ({title, option1, option2, Id, IsSend, taskList}) => {
    const sendAgenda = () => {
        const taskInfo = taskList.find(x=>x.Id == Id);
        taskInfo.IsSend = true;
            fetch("/api/sendAgenda", {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                  },
                body: JSON.stringify({taskInfo, taskList}),
            })
    };
    const submitHandler = (dialogResponse) => {
        return true;
    };
    
    const openResultModule = () => {
        const baseUrl = `https://${window.location.hostname}:${window.location.port}`;
        let taskInfo = {
            title: null,
            size: null,
            url: null,
            fallbackUrl: null,
        };

        taskInfo.url = baseUrl +"/Result?id="+Id;
        taskInfo.title = "Result";
        taskInfo.size = {
            height: 250,
            width: 500,
        };
        taskInfo.fallbackUrl = taskInfo.url

        microsoftTeams.app.initialize().then(() => {
            microsoftTeams.dialog.open(taskInfo, submitHandler);
        });
    }
    if(!IsSend){
    return (
        <div className="card agendaCard">
            <div className="card-body">
                <h5 className="card-title">{title}</h5>
                <input type="radio" className="option1" name="option1" value={option1}/>
                <label className="pollLabel" for="option1">{option1}</label><br/>
                <input type="radio" className="option2" name="option2" value={option2}/>
                <label className="pollLabel" for="option2">{option2}</label><br/>
            </div>
            <div className="card-footer">
                <button type="button" className="btn btn-primary" onClick={() => sendAgenda(title, option1, option2)}>Send</button>
            </div>
        </div>
        )
    } else {
        const taskInfo = taskList.find(x=>x.Id == Id);
        const {title, option1, option2, personAnswered}  = taskInfo;
        const option1Answered = personAnswered && personAnswered[option1] ? personAnswered[option1].length : 0;
        const option2Answered = personAnswered && personAnswered[option2] ? personAnswered[option2].length : 0;

        const total = option1Answered + option2Answered;
        const percentOption1 = total == 0 ? 0 : parseInt(( option1Answered * 100 ) / total);
        const percentOption2 = total == 0 ? 0 : 100 - percentOption1;
        
        return (
        <div class="card agendaCard">
                <div class="card-body">
                    <h5 class="card-title">{title}</h5>
                    <span class="option1">{option1}</span><br/>
                    <span class="resultpercentage">{`${percentOption1}% has answerd`} </span><br />
                    <span class="option2">{option2}</span><br/>
                    <span class="resultpercentage">{`${percentOption2}% has answerd`} </span>
                </div>
                <div class="card-footer">
                    <button type="button" class="btn btn-primary btnResult" onClick={() => openResultModule()}>Results</button>
                </div>
        </div>
        )
    }
};
export default Agenda;