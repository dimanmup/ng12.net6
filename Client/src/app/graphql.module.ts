import { NgModule } from '@angular/core';
import { APOLLO_OPTIONS } from 'apollo-angular';
import { ApolloClientOptions, InMemoryCache } from '@apollo/client/core';
import { HttpLink } from 'apollo-angular/http';
import { HttpClientModule } from '@angular/common/http';
import { environment } from 'src/environments/environment';

const uri = environment.uriRoot + 'graphql';
export function createApollo(httpLink: HttpLink): ApolloClientOptions<any> {
  return {
    link: httpLink.create({ uri }),
    cache: new InMemoryCache(),
  };
}

@NgModule({
  imports: [
    HttpClientModule
  ],
  providers: [
    {
      provide: APOLLO_OPTIONS,
      //useFactory: createApollo,
      useFactory(httpLink: HttpLink) {
        return {
          cache: new InMemoryCache({
            typePolicies: {
            }
          }),
          link: httpLink.create({
            uri,
            withCredentials: true
          })
        };
      },
      deps: [HttpLink],
    },
  ],
})
export class GraphQLModule { }
