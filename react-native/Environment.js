
const yourIP = 'Your Local IP Address etc 192.168.1.64'; // See the docs https://docs.abp.io/en/abp/latest/Getting-Started-React-Native?Tiered=No
const port  = 44305;
const apiUrl = `http://${yourIP}:${port}`;
const ENV = {
  dev: {
    apiUrl: apiUrl,
    oAuthConfig: {
      issuer: apiUrl,
      clientId: 'BookStore_App',
      scope: 'offline_access BookStore',
    },
    localization: {
      defaultResourceName: 'BookStore',
    },
  },
  prod: {
    apiUrl: 'http://localhost:44356',
    oAuthConfig: {
      issuer: 'http://localhost:44356',
      clientId: 'BookStore_App',
      scope: 'offline_access BookStore',
    },
    localization: {
      defaultResourceName: 'BookStore',
    },
  },
};

export const getEnvVars = () => {
  // eslint-disable-next-line no-undef
  return __DEV__ ? ENV.dev : ENV.prod;
};
