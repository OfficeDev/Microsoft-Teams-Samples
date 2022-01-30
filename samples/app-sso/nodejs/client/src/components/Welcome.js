import React, { useState, useContext } from 'react';
import TeamsContext from '../context/TeamsContext';
import {getUserProfile, getUserPhoto} from '../service/TeamsClientService'

function Welcome (props){
    const { teamsContext, authCode } = useContext(TeamsContext);
    const [ profile, setProfile ] = useState('')
    const [ photo, setPhoto ] = useState('')
    const [ flag, setFlag ] = useState(false)
    if(teamsContext && authCode){
        if(!flag) {
            getUserProfile(authCode, teamsContext).then(res=>{
                setProfile(res.data);
                console.log(res.data);
            });
            getUserPhoto(authCode, teamsContext).then(res=>{
                setPhoto(res.data);
            });
            setFlag(true);
        }
    }
    
    return (
        <div>
            <div style={{padding: '10px'}}>
                <br></br>
                <div style={{textAlign: 'center'}}>
                    <h3><b >Teams Tab SSO</b></h3>
                </div>
                <hr></hr><br></br>
                <div><h4><b>User Details</b></h4></div>
                <img src={photo} width="100" height="100" align="right" style={{marginRight: '50px'}} alt='Loading...'></img>
                <div>
                    <table style={{width: '100%', borderSpacing: '100px 20px'}}>
                        <tbody>
                            <tr style={{padding: '60px'}}>
                                <td>User Name</td>
                                <td>{profile.displayName}</td>
                            </tr>
                            <tr>
                                <td>Email</td>
                                <td>{profile.mail}</td>
                            </tr>
                            <tr>
                                <td>Office Location</td>
                                <td>{profile.officeLocation}</td>
                            </tr>
                            <tr>
                                <td>Job Title</td>
                                <td>{profile.jobTitle}</td>
                            </tr>
                            <tr>
                                <td>Preferred Language</td>
                                <td>{profile.preferredLanguage}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    )
}

export default Welcome