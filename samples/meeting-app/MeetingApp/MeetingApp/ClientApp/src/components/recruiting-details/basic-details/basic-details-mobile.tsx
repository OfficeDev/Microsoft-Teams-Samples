import * as React from 'react';
import { Flex, Card, Avatar, Text, Header, Button, Label } from '@fluentui/react-northstar'
import "../../recruiting-details/recruiting-details.css"
import LinkedInLogo from '../../../images/linkedin.svg';
import TwitterLogo from '../../../images/twitter.svg';
import { getCandidateDetails } from "../services/recruiting-detail.service"
import { ICandidateDetails } from './basic-details.types';

export interface IBasicDetailsMobileProps {
    selectedIndex: number,
}

const BasicDetailsMobile = (props: IBasicDetailsMobileProps) => {
    const links = [{
        type: "Resume",
        url: ""
    },
    {
        type: "Peer feedback",
        url: ""
    }
    ]

    const [candidateDetails, setCandidateDetails] = React.useState<ICandidateDetails[]>([]);
    const [skills, setSkills] = React.useState<string[]>([]);

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
                            <Text content={candidateDetails[props.selectedIndex]?.candidateName} />
                            <Text content={candidateDetails[props.selectedIndex]?.role} size="small" className="roleText" />
                        </Flex>
                    </Flex>
                    <Flex column>
                        <Flex gap="gap.small">
                            <Text content="Experience" size="small" className="expLabel" />
                            <Text content={candidateDetails[props.selectedIndex]?.experience} size="small" />
                        </Flex>
                        <Flex gap="gap.small">
                            <Text content="Education" size="small" />
                            <Text content={candidateDetails[props.selectedIndex]?.education} size="small" className="education" />
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
                            <img src={LinkedInLogo} alt="Linked in icon" />
                            <img src={TwitterLogo} alt="Twitter icon" />
                        </Flex>
                        <Flex gap="gap.small">
                            {links.map((link, index) => {
                                return (
                                    <Label content={link.type} className="linkLabel" />
                                )
                            })
                            }
                        </Flex>
                    </Flex>
                </Flex>
            </Card.Body>
        </Card>
    )
}

export default (BasicDetailsMobile);