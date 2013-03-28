using System.Collections.Generic;
using System.Linq;
using Core.Planing.PrioritizedCollections;
using Core.PlaningActions;
using Core.States;

namespace Core.Planing
{
    public static class Planner
    {
        public static IEnumerable<IState<T>> MakePlan<T>(IState<T> initialState, IState<T> goalState, IEnumerable<PlanningAction<T>> planningActions, Method method)
        {
            var visited = new HashSet<IState<T>>();
            var states = UnvisitedStates<Path<IState<T>>>(method);
            states.Add(0, new Path<IState<T>>(initialState));
            while (states.HasElements)
            {
                Path<IState<T>> path = states.Get();
                bool already = visited.Contains(path.Node);
                if (already) continue;
                if (path.Node.Equals(goalState)) return path;

                visited.Add(path.Node);

                var actions = planningActions.Where(a => a.CanExecute(path.Node));

                var posibleStates = actions.Select(a => a.Migrate(path.Node));

                foreach (var state in posibleStates)
                {
                    var newPlan = path.Add(state, state.Distance(path.Node));
                    states.Add(newPlan.PathCost + newPlan.Node.Distance(goalState), newPlan);
                }
            }
            return null;
        }

        private static IPrioritized<double, S> UnvisitedStates<S>(Method method)
        {
            IPrioritized<double, S> prioritized = null;
            switch (method)
            {
                case Method.BreadthFirst:
                    prioritized = new PrioritizedQueue<double, S>();
                    break;
                case Method.DepthFirst:
                    prioritized = new PrioritizedStack<double, S>();
                    break;
            }
            return prioritized;
        }
    }
}