
// Returns Response in the selected Teams language
const GetTranslatedRes = (lanCd)=> {
    try {
        return require(`../translations/${lanCd}/common.json`)
    } catch (error) {
        return require(`../translations/en-us/common.json`)
    }
}

module.exports = {
    GetTranslatedRes
}