import React,{useState, useEffect} from "react";
const Result = (props) => {
    const [taskInfo, setInfo]= useState(null);
    useEffect(() => {
        const loadAgenda = async () => {
            const response = await fetch("/api/getAgendaList", {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                  }
            });
            const data = await response.json();
            const params = props.location.search.split("=");
            const id = params[1];
            const info = data.find(x=> x.Id === id);
            console.log(info);
            setInfo(info);
        };
        loadAgenda();
    }, []);
    if(taskInfo != null){
        const {option1, option2, personAnswered}  = taskInfo;
        const option1Answers = personAnswered && personAnswered[option1] ? personAnswered[option1] : [];
        const option2Answers = personAnswered && personAnswered[option2] ? personAnswered[option2] : [];
    return (
        <div class="card" id="getResultsModal" tabindex="-1">
            <div class="card-header">
                Users Answered the poll
            </div>
            <div class="card-body">
                <span class="option1">{option1}</span><br/>
                <span class="resultNames">{option1Answers.join(", ")}</span><br />
                <span class="option2">{option2}</span><br/>
                <span class="resultNames">{option2Answers.join(", ")}</span>
            </div>
        </div>
        )
    }else {
        return (
            <div>
                Loading......
            </div>
        )
    }
    
}
export default Result;