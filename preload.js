// preload.js
const { contextBridge, ipcRenderer } = require('electron');

const validInvokeChannels = new Set([
  'get-users',
  'insert-user',
  'post-data',
  'update-data',
  'get-data-by-filters',
  'delete-data',
  'insert-data',
  'serial-send-data',
  'serial-get-status',
  'serial-reconnect'
]);

const validReceiveChannels = new Set([
  'serial-data-received',
  'serial-port-status',
  'serial-port-error',
  'serial-data-sent',
  'database-insert-success'
]);

contextBridge.exposeInMainWorld('api', {
  invoke: (channel, ...args) => {
    if (!validInvokeChannels.has(channel)) {
      console.warn(`Invalid invoke channel: ${channel}`);
      return Promise.reject(new Error(`Invalid channel: ${channel}`));
    }
    return ipcRenderer.invoke(channel, ...args);
  },

  receive: (channel, callback) => {
    if (!validReceiveChannels.has(channel)) {
      console.warn(`Invalid receive channel: ${channel}`);
      return;
    }
    ipcRenderer.on(channel, (_, ...args) => callback(...args));
  },

  removeAllListeners: (channel) => {
    if (validReceiveChannels.has(channel)) {
      ipcRenderer.removeAllListeners(channel);
    }
  },

  // Helper methods for serial communication
  serial: {
    onDataReceived: (callback) => {
      ipcRenderer.on('serial-data-received', (_, data) => callback(data));
    },
    onStatusChange: (callback) => {
      ipcRenderer.on('serial-port-status', (_, status) => callback(status));
    },
    onError: (callback) => {
      ipcRenderer.on('serial-port-error', (_, error) => callback(error));
    },
    sendData: (data) => {
      return ipcRenderer.invoke('serial-send-data', data);
    },
    getStatus: () => {
      return ipcRenderer.invoke('serial-get-status');
    },
    reconnect: () => {
      return ipcRenderer.invoke('serial-reconnect');
    }
  }
});

// Debug helper
contextBridge.exposeInMainWorld('debugApi', {
  log: (...args) => console.log('[Renderer Debug]:', ...args),
  error: (...args) => console.error('[Renderer Debug]:', ...args)
});