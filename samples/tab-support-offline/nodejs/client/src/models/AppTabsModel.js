import StagedIssuesTab from '../components/tabContents/StagedIssuesTab';
import LiveIssuesTab from '../components/tabContents/LiveIssuesTab';

export const AppTabs = [
    { title: 'Staged', key: 'staged' },
    { title: 'Live Issues', key: 'live' }
];

export const componentForTab = (tab, selectedFlight, actionHandler) => {
    switch (tab.key) {
        case 'staged':
            return (<StagedIssuesTab selectedFlight={selectedFlight} actionHandler={actionHandler} />);
        case 'live':
            return (<LiveIssuesTab selectedFlight={selectedFlight} actionHandler={actionHandler}/>);
        default:
            return null;
    }
}


