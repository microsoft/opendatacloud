// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Pipe, PipeTransform } from '@angular/core';

/**
 * Convert Enum into its string value.
 */
@Pipe({ name: 'enum' })
export class EnumPipe implements PipeTransform {
    transform(value): any {
        const keys = [];
        for (const enumMember in value) {
            if (!isNaN(parseInt(enumMember, 10))) {
                keys.push({ key: enumMember, value: (value[enumMember]).replace(/([A-Z])/g, ' $1').trim() });
            }
        }
        return keys;
    }
}
