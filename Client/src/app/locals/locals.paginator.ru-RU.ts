import { Injectable } from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';

@Injectable()
export class MatPaginatorIntl_ruRU extends MatPaginatorIntl {
  
  constructor() {
    super();  
    this.getAndInitTranslations();
  }

  getAndInitTranslations() {
      this.itemsPerPageLabel = 'записей на странице';
      this.nextPageLabel = 'следующая страница';
      this.previousPageLabel = 'предыдущая страница';
      this.firstPageLabel = 'первая страница';
      this.lastPageLabel = 'последняя страница';
      this.changes.next();
  }

 getRangeLabel = (page: number, pageSize: number, length: number) =>  {
    
    if (length === 0 || pageSize === 0)
      return `0 из ${length}`;
    
    // length = Math.max(length, 0);
    // const startIndex = page * pageSize;
    // const endIndex = startIndex < length ? Math.min(startIndex + pageSize, length) : startIndex + pageSize;
    // return `${startIndex + 1} - ${endIndex} из ${length}`;

    const numberOfPages = Math.ceil(length / pageSize);
    return `страница ${page + 1} из ${numberOfPages}`;
  }
}