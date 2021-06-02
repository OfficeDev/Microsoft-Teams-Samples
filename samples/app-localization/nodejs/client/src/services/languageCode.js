// Converts MS Language Code
const LangCode = (code)=>{
    const supported_langs = ['en_us', 'es_mx', 'hi_in']
    if(!code) return supported_langs[0];
    let lanCode = code.replace('-', '_').toLowerCase()
    
    let lan_supported = supported_langs.find(l => l === lanCode)
    if(lan_supported)
        return lanCode
    else 
        return supported_langs[0];
}

module.exports = {
    LangCode
}