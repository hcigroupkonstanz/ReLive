import { Service } from '../core';
import { WebServer } from './web-server';
import { Router } from 'express';
import * as fs from 'fs';

const STORE_FILENAME = 'store.json';

export class RestAPI extends Service {
    public serviceName = 'RestAPI';
    public groupName = 'web';

    private data: any = {};
    private storeFilePath: string;

    public constructor(dataPath: string, webserver: WebServer) {
        super();

        this.storeFilePath = dataPath + STORE_FILENAME;

        webserver.addApi('/store', Router()
            .get('/', (req, res) => {
                // Return all available apps
                const keys = Object.keys(this.data);
                res.status(200).json(keys);
            })
            .get('/:app', (req, res) => {
                const app = this.data[req.params.app];
                // If app not exist return an error
                if (app == null) {
                    res.status(404).json({ error: 'App with name ' + req.params.app + ' not found' });
                } else {
                    // Return all available values of the app
                    const keys = Object.keys(app);
                    res.status(200).json(keys);
                }
            })
            .get('/:app/:value', (req, res) => {
                const app = this.data[req.params.app];
                // If app not exist return an error
                if (app == null) {
                    res.status(404).json({ error: 'App with name ' + req.params.app + ' not found' });
                } else {
                    const value = app[req.params.value];
                    // If value not exist return an error
                    if (value == null) {
                        res.status(404).json({ error: 'Value with name ' + req.params.value + ' not found' });
                    } else {
                        // Return data of value
                        res.status(200).json(value);
                    }
                }
            })
            .put('/:app/:value', (req, res) => {
                let statusCode = 200;
                // If app name not exist create app name
                if (this.data[req.params.app] == null) {
                    this.data[req.params.app] = {};
                    statusCode = 201;
                } else if (this.data[req.params.app][req.params.value] == null) {
                    statusCode = 201;
                }
                // Set data to the corresponding value
                this.data[req.params.app][req.params.value] = req.body;
                // Return successful result with the sended data
                res.status(statusCode).json({ result: 'Value with name ' + req.params.value + ' saved successfully', data: this.data[req.params.app][req.params.value] });
                // Save data in data store file
                this.saveData();
            })
            .delete('/:app', (req, res) => {
                const app = this.data[req.params.app];
                // If app not exist return an error
                if (app == null) {
                    res.status(404).json({ error: 'App with name ' + req.params.app + ' not found' });
                } else {
                    // Delete the app
                    delete this.data[req.params.app];
                    // Return successful result
                    res.status(200).json({ result: 'App with name ' + req.params.app + ' deleted successfully' });
                    // Save data in data store file
                    this.saveData();
                }
            })
            .delete('/:app/:value', (req, res) => {
                const app = this.data[req.params.app];
                // If app not exist return an error
                if (app == null) {
                    res.status(404).json({ error: 'App with name ' + req.params.app + ' not found' });
                } else {
                    const value = app[req.params.value];
                    // If value not exist return an error
                    if (value == null) {
                        res.status(404).json({ error: 'Value with name ' + req.params.value + ' not found' });
                    } else {
                        // Delete the value
                        delete this.data[req.params.app][req.params.value];
                        // Return successful result
                        res.status(200).json({ result: 'Value with name ' + req.params.value + ' deleted successfully' });
                        // Save data in data store file
                        this.saveData();
                    }
                }
            }));

        // Read data in data store file
        this.readData();
    }

    private async readData() {
        fs.stat(this.storeFilePath, (err, stat) => {
            const fileExists = !err;

            if (fileExists) {
                fs.readFile(this.storeFilePath, 'utf8', (readErr, data) => {
                    if (readErr) {
                        this.logError(readErr.message, false);
                    } else {
                        const obj = JSON.parse(data);
                        this.data = obj;
                    }
                });
            }

        });

    }

    private saveData() {
        const fileContent = JSON.stringify(this.data);
        const options: fs.WriteFileOptions = {
            encoding: 'utf8'
        };
        fs.writeFile(this.storeFilePath, fileContent, options, (err) => {
            if (err) {
                this.logError(err.message, false);
            }
        });
    }
}
