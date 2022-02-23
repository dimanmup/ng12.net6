import { HttpErrorResponse } from '@angular/common/http';
import { Component, Inject, OnInit } from '@angular/core';
import { MatSnackBarRef, MAT_SNACK_BAR_DATA } from '@angular/material/snack-bar';
import { ApolloError } from '@apollo/client/errors';

@Component({
  selector: 'app-error',
  templateUrl: './error.component.html',
  styleUrls: ['./error.component.less']
})
export class ErrorComponent implements OnInit {

  title: string = 'Unknown error';
  text: string = 'Open the browser console (F12).';

  constructor(@Inject(MAT_SNACK_BAR_DATA) public data: HttpErrorResponse | ApolloError | any, private snackRef: MatSnackBarRef<ErrorComponent>) {

    console.log({data});

    if (data instanceof ApolloError) {

      this.title = 'Apollo error stack';

      const textLines : string[] = [];

      if (data.networkError)
        textLines.push('[network error] ' + data.networkError.message);

      if (data.clientErrors)
        data.clientErrors.map(ce => '[client error] ' + ce.message).forEach(m => textLines.push(m));

      if (data.graphQLErrors) 
        data.graphQLErrors.map(ge => `[graphQL error (code: ${ge.extensions?.code})] ${ge.message}`).forEach(m => textLines.push(m));

      this.text = textLines.join('\n\n');

    } else if (data instanceof HttpErrorResponse) {

      this.title = `${data.statusText} (${data.status})`;

      if(data.error && (typeof data.error === 'string' || data.error instanceof Array))
        this.text = [data.error].join('\n\n');
      else
        this.text = data.message;

    }
  }
  
  dismiss() {
    this.snackRef.dismiss();
  }

  ngOnInit(): void {
  }

}
