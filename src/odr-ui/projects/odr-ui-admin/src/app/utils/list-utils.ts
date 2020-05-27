
import {uniqBy, orderBy, differenceBy} from 'lodash';

const cmpFn = n => n.toLowerCase();

export function addToSortedList(list, value) {
    if (typeof(value) === 'string') {
        list = orderBy(uniqBy([...list, value], cmpFn), cmpFn);
    }
    return list;
}

export function removeFromSortedList(list, value) {
    if (typeof(value) === 'string') {
        list = differenceBy(list, [value], cmpFn);
    }
    return list;
}

export function matchListId(item1, item2) {
    return item1 && item2 ? item1.id === item2.id : item1 === item2;
}
