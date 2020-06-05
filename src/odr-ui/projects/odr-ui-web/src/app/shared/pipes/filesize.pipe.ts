// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Pipe, PipeTransform } from '@angular/core';

/**
 * Convert filesize to be in B/KB/MB/GB format
 */
@Pipe({ name: 'filesize' })
export class FileSizePipe implements PipeTransform {
    transform(value): any {
        if (!value || value === 0) {
            return '';
        }

        value = Number(value);

        if (isNaN(value)) {
            return '';
        }

        const kbInBytes = 1000;
        const mbInBytes = 1000000;
        const gbInBytes = 1000000000;
        let unit = 'B';

        const gb = value / gbInBytes;
        if (gb > 1) {
            unit = 'GB';
            return (gb % 1 === 0) ?
                `${gb.toFixed(0)} ${unit}` :
                `${gb.toFixed(2)} ${unit}`;
        }

        const mb = value / mbInBytes;
        if (mb > 1) {
            unit = 'MB';
            return (mb % 1 === 0) ?
                `${mb.toFixed(0)} ${unit}` :
                `${mb.toFixed(2)} ${unit}`;
        }

        const kb = value / kbInBytes;
        if (kb > 1) {
            unit = 'KB';
            return (kb % 1 === 0) ?
                `${kb.toFixed(0)} ${unit}` :
                `${kb.toFixed(2)} ${unit}`;
        }

        return (value % 1 === 0) ?
            `${value.toFixed(0)} ${unit}` :
            `${value.toFixed(2)} ${unit}`;
    }
}
