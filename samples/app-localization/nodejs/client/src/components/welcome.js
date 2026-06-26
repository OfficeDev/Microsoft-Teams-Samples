// Importing the useTranslation hook from react-i18next for handling translations
import {useTranslation} from "react-i18next";
// Importing LangCode function from a local service that converts culture to language code
import {LangCode} from '../services/languageCode'

// Define a functional component named Welcome that receives props
function Welcome (props){
    // Extracting the 't' function and 'i18n' object from useTranslation hook for localization and language handling
    const {t, i18n} = useTranslation('common');
    
    // Parsing the query parameters from the URL, specifically the 'culture' parameter
    const query = new URLSearchParams(decodeURIComponent(props.location.search))
    
    // Changing the language using the 'culture' query parameter and the LangCode function
    i18n.changeLanguage(LangCode(query.get('culture')))
    
    // Rendering a div with a translated welcome message
    return (
        <div>
            {/* Displaying a translated 'welcome.title' from the common namespace */}
            <div>{t('welcome.title')}</div>
        </div>
    )
}

// Exporting the Welcome component to be used elsewhere
export default Welcome
