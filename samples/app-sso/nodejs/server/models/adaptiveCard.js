const profileCard = (profileName, profileImage) => ({
    version: '1.0.0',
    type: 'AdaptiveCard',
    body: [
        {
            type: 'Image',
            url: `${ profileImage }`,
            size: 'small',
            style: 'person',
            backgroundColor: '#e0e0e0'
        }, {
            type: 'TextBlock',
            text: `Hello ${ profileName }`
        }
    ]
});

const signedOutCard = () => ({
    version: '1.0.0',
    type: 'AdaptiveCard',
    body: [
        {
            type: 'TextBlock',
            text: 'You have been signed out.'
        }
    ],
    actions: [
        {
            type: 'Action.Submit',
            title: 'Close',
            data: {
                key: 'close'
            }
        }
    ]
});

module.exports = {
    profileCard,
    signedOutCard
};
