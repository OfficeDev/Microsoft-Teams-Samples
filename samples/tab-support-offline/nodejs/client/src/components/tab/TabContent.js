
import {componentForTab} from '../../models/AppTabsModel'

const TabContent = ({ tab, key, selectedFlight, actionHandler }) => {
    return (
        componentForTab(tab, selectedFlight, actionHandler)
    );
};

export { TabContent };