import { Pipe, PipeTransform } from '@angular/core';

/**
 * add commas between elements of an array
 */
@Pipe({ name: 'comma_delimited' })
export class CommaDelimitedPipe implements PipeTransform {
    transform(value): any {

        if (!value) {
            return '';
        }

        if (!Array.isArray(value)) {
            return value;
        }

        let result = '';
        for (let i = 0; i < value.length; i++) {
            result += `${value[i]}, `;
        }

        result = result.trim();
        if (result.endsWith(',')) {
            return result.substr(0, result.length - 1);
        }

        return result;
    }
}
