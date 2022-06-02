import path from 'path';
import winston from 'winston';
import dotenv from 'dotenv';
import fs from 'fs';

// FIXME: duplicate with server.ts because it's already needed here
if (fs.existsSync('.env')) {
    // tslint:disable-next-line:no-console
    dotenv.config({ path: '.env' });
}

const options: winston.LoggerOptions = {
    transports: [
        new winston.transports.Console({
            level: process.env.NODE_ENV === 'production' ? 'error' : 'debug',
            format: winston.format.combine(
                winston.format.colorize(), 
                winston.format.printf(info => `${new Date().toISOString()}-${info.level}: ${JSON.stringify(info.message, null, 4)}`)
            ),
        }),
        new winston.transports.File({
            filename: path.join(process.env.PERSISTENT_FILE_PATH, process.env.SESSION_ID + '.log'),
            level: 'debug',
            format: winston.format.combine(
                winston.format.timestamp({
                    format: 'YYYY-MM-DD hh:mm:ss.sss A ZZ'
                }),
                winston.format.json()
            ),
        })
    ]
};

const logger = winston.createLogger(options);

if (process.env.NODE_ENV !== 'production') {
    logger.debug('Logging initialized at debug level');
}


export default logger;
