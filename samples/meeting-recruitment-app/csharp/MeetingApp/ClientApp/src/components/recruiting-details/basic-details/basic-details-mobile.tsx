import * as React from 'react';
import { Flex, Card, Avatar, Text, Header, Button, Label, AddIcon } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import LinkedInLogo from '../../../images/linkedin.svg';
import TwitterLogo from '../../../images/twitter.svg';
import { getCandidateDetails, shareAssets } from "../services/recruiting-detail.service"
import { IAssetDetails, ICandidateDetails } from './basic-details.types';
import * as microsoftTeams from "@microsoft/teams-js";

export interface IBasicDetailsMobileProps {
    selectedIndex: number,
    downloadFile: () => void,
}

const BasicDetailsMobile = (props: IBasicDetailsMobileProps) => {
    const [candidateDetails, setCandidateDetails] = React.useState<ICandidateDetails[]>([]);
    const [skills, setSkills] = React.useState<string[]>([]);
    const [selectedIndex, setSelectedIndex] = React.useState(props.selectedIndex);

    const openShareTaskModule = () => {
        let taskInfo = {
            title: "Share policy assets",
            height: 400,
            width: 400,
            url: `${window.location.origin}/shareAssets`,
        };

        microsoftTeams.tasks.startTask(taskInfo, (err: any, note: any) => {
            if (err) {
                console.log("Some error occurred in the task module")
                return
            }
            var details = JSON.parse(note);
            let files = new Array();
            details.checkedValues.map((item: any) => {
                if (item.isChecked == true) {
                    files.push(item.name);
                }
            })
            if (note !== undefined) {
                microsoftTeams.getContext((context) => {
                    const assetDetail: IAssetDetails = {
                        message: details.note,
                        sharedBy: context.userPrincipalName!,
                        meetingId: context.meetingId!,
                        files: files,
                    }
                    shareAssets(assetDetail)
                        .then((res) => {
                            console.log(res)
                        })
                        .catch((ex) => {
                            console.log("Some error occurred while sharing the assets info");
                            console.log(ex);
                        });
                })
            }
        })
    }

    React.useEffect(() => {
        getCandidateDetails()
            .then((res) => {
                console.log(res)
                const data = res.data as ICandidateDetails[];
                setSkills(data[props.selectedIndex].skills.split(','));
                setCandidateDetails(data);
            })
            .catch((ex) => {
                console.log(ex)
            });
    }, [])

    React.useEffect( () => {
        setSelectedIndex(props.selectedIndex);
    }, [props.selectedIndex])


    return (
        <Card fluid aria-roledescription="card with basic details" className="basic-details-card-mobile">
            <Card.Header>
                <Text content="Candidate Details" />
            </Card.Header>
            <Card.Body>
                <Flex gap="gap.small" padding="padding.medium" column className="basicDetails">
                    <Flex gap="gap.small">
                        <Avatar
                            image="https://fabricweb.azureedge.net/fabric-website/assets/images/avatar/large/jenny.jpg"
                            label="Copy bandwidth"
                            name="Evie yundt"
                            status="unknown"
                        />
                        <Flex column>
                            <Text content={candidateDetails[selectedIndex]?.candidateName} />
                            <Text content={candidateDetails[selectedIndex]?.role} size="small" className="roleText" />
                        </Flex>
                    </Flex>
                    <Flex column>
                        <Flex gap="gap.small">
                            <Text content="Experience" size="small" className="expLabel" />
                            <Text content={candidateDetails[selectedIndex]?.experience} size="small" />
                        </Flex>
                        <Flex gap="gap.small">
                            <Text content="Education" size="small" />
                            <Text content={candidateDetails[selectedIndex]?.education} size="small" className="education" />
                        </Flex>
                    </Flex>
                    <Flex column>
                        <Header as="h5" content="Skills" className="subHeaders" />
                        <Flex gap="gap.small">
                            {skills.map((skill, index) => {
                                return (
                                    <Label circular content={skill} className="skillLabel" />
                                )
                            })
                            }
                        </Flex>
                    </Flex>
                    <Flex column>
                        <Header as="h5" content="Links" className="subHeaders" />
                        <Flex gap="gap.small" className="linkIcons">
                            <img src={LinkedInLogo} alt="Linked in icon" onClick={() => {
                                window.open(candidateDetails[selectedIndex].linkedInUrl)
                            }} />
                            <img src={TwitterLogo} alt="Twitter icon" onClick={() => {
                                window.open(candidateDetails[selectedIndex].twitterUrl)
                            }} />
                        </Flex>
                        <Flex gap="gap.small">
                            <Button content={"Resume"} className="linkLabel" onClick={props.downloadFile} />
                            <Button content={"Peer feedback"} className="linkLabel" />
                        </Flex>
                    </Flex>
                    <Flex>
                        <Button
                            icon={<AddIcon />}
                            content={'Share docs'}
                            size="small"
                            className="shareDocs"
                            onClick={openShareTaskModule} />
                    </Flex>
                </Flex>
            </Card.Body>
        </Card>
    )
}
export default (BasicDetailsMobile);