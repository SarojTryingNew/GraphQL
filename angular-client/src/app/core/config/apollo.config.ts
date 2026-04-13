import { ApplicationConfig } from '@angular/core';
import { Apollo, APOLLO_OPTIONS } from 'apollo-angular';
import { HttpLink } from 'apollo-angular/http';
import { ApolloClientOptions, InMemoryCache } from '@apollo/client/core';
import { environment } from '../../../environments/environment';

export function apolloOptionsFactory(httpLink: HttpLink): ApolloClientOptions {
  return {
    link: httpLink.create({ uri: environment.graphqlEndpoint }),
    cache: new InMemoryCache({
      typePolicies: {
        Query: {
          fields: {
            orders: {
              merge: false // Don't merge arrays, always replace
            }
          }
        }
      }
    }),
    defaultOptions: {
      watchQuery: {
        fetchPolicy: 'network-only', // Always fetch fresh data for comparison
        errorPolicy: 'all'
      },
      query: {
        fetchPolicy: 'network-only',
        errorPolicy: 'all'
      },
      mutate: {
        errorPolicy: 'all'
      }
    }
  };
}

export function provideApollo(): ApplicationConfig['providers'] {
  return [
    Apollo,
    {
      provide: APOLLO_OPTIONS,
      useFactory: apolloOptionsFactory,
      deps: [HttpLink]
    }
  ];
}
