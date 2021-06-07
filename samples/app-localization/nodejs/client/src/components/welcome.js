import {useTranslation} from "react-i18next";
import {LangCode} from '../services/languageCode'

function Welcome (props){
    const {t, i18n} = useTranslation('common');
    const query = new URLSearchParams(decodeURIComponent(props.location.search))
    i18n.changeLanguage(LangCode(query.get('culture')))
    return (
        <div>
            <div>{t('welcome.title')}</div>
        </div>
    )
}

export default Welcome